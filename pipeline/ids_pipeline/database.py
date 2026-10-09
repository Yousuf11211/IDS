"""SQL Server production store; SQLite is an isolated development/test store."""
from contextlib import contextmanager
from datetime import datetime, timezone
import os
import sqlite3

from .schema import COLUMNS, NAMES


def utcnow():
    # Explicit ISO serialization avoids sqlite3's deprecated datetime adapter.
    # SQL Server datetime2 accepts the same ISO-8601 representation.
    return datetime.now(timezone.utc).replace(tzinfo=None).isoformat()


class Store:
    def __init__(self, config):
        self.config = config
        self.sqlite = config.backend == "sqlite"
        self.lock_file = None
        if self.sqlite:
            path = config.root / config.sqlite_path
            path.parent.mkdir(parents=True, exist_ok=True)
            self.conn = sqlite3.connect(path)
            self.conn.execute("PRAGMA foreign_keys=ON")
            self.conn.execute("PRAGMA journal_mode=WAL")
            self.conn.execute("PRAGMA busy_timeout=5000")
        else:
            import pyodbc
            # A closed session must release its application lock, not enter a pool.
            pyodbc.pooling = False
            connection = os.environ.get(config.connection_env)
            if not connection:
                raise ValueError(f"Set {config.connection_env} to an ODBC connection string")
            self.conn = pyodbc.connect(connection, autocommit=False, timeout=15)
            self.conn.timeout = 60
            self.conn.execute("SET XACT_ABORT ON")
            self.conn.commit()

    def table(self, name):
        return f"[{name}]" if self.sqlite else f"[dbo].[{name}]"

    @contextmanager
    def transaction(self):
        try:
            yield
            self.conn.commit()
        except BaseException:
            self.conn.rollback()
            raise

    def close(self):
        self.conn.close()
        if self.lock_file:
            self.lock_file.close()  # OS releases lock on close/process exit.

    def acquire_lock(self):
        """One worker per database. Recovery needs no expiring per-row leases."""
        if self.sqlite:
            path = self.config.root / (self.config.sqlite_path + ".lock")
            self.lock_file = path.open("a+b")
            self.lock_file.write(b"0")
            self.lock_file.flush()
            self.lock_file.seek(0)
            try:
                if os.name == "nt":
                    import msvcrt
                    msvcrt.locking(self.lock_file.fileno(), msvcrt.LK_NBLCK, 1)
                else:
                    import fcntl
                    fcntl.flock(self.lock_file, fcntl.LOCK_EX | fcntl.LOCK_NB)
            except OSError:
                raise RuntimeError("Another worker owns this database") from None
        else:
            cursor = self.conn.execute("""
                SET NOCOUNT ON;
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource='IDS.CSV.Pipeline.v1',
                    @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=0;
                SELECT @result;
            """)
            result = cursor.fetchone()[0]
            self.conn.commit()
            if result < 0:
                raise RuntimeError("Another worker owns this database, or application lock was denied")

    def initialize(self):
        if not self.sqlite:
            with self.transaction():
                self.conn.execute((self.config.root / "database/001_pipeline.sql").read_text())
            return
        fields = []
        for column in COLUMNS:
            kind = column["type"]
            sql_type = "INTEGER" if kind in ("int", "long") else "REAL" if kind == "double" else "TEXT"
            fields.append(f'[{column["name"]}] {sql_type}')
        common = ",".join(fields)
        self.conn.executescript(f"""
            CREATE TABLE IF NOT EXISTS RawPackets (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                {common}, IsProcessed INTEGER NOT NULL, Classification TEXT, ClassifiedRecordId INTEGER);
            CREATE TABLE IF NOT EXISTS Benign_Table (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                {common}, ConfidenceScore REAL NOT NULL, ModelVersion TEXT);
            CREATE TABLE IF NOT EXISTS Attack_Table (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                {common}, ConfidenceScore REAL NOT NULL, ModelVersion TEXT,
                AttackType TEXT NOT NULL, AttackCategory TEXT, Severity TEXT NOT NULL,
                IsAcknowledged INTEGER NOT NULL, AcknowledgedBy TEXT, AcknowledgedAt TEXT, Notes TEXT);
            CREATE TABLE IF NOT EXISTS PipelineImports (
                FileHash TEXT PRIMARY KEY, FileName TEXT NOT NULL, Release TEXT NOT NULL,
                Fingerprint TEXT NOT NULL, GateVersion TEXT NOT NULL, TypeVersion TEXT NOT NULL,
                Status TEXT NOT NULL, LastRow INTEGER NOT NULL DEFAULT 0,
                Attempts INTEGER NOT NULL DEFAULT 0, NextAttempt REAL NOT NULL DEFAULT 0,
                LastError TEXT, CreatedAt TEXT NOT NULL, UpdatedAt TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS PipelineRows (
                FileHash TEXT NOT NULL REFERENCES PipelineImports(FileHash), RowNumber INTEGER NOT NULL,
                RawId INTEGER UNIQUE REFERENCES RawPackets(Id), State TEXT NOT NULL, Error TEXT,
                GateScore REAL, TypeScore REAL, ResultId INTEGER, ProcessedAt TEXT,
                PRIMARY KEY (FileHash, RowNumber));
            CREATE INDEX IF NOT EXISTS IX_PipelineRows_Pending ON PipelineRows(FileHash, State, RowNumber);
        """)
        self.conn.commit()

    def verify(self):
        # Check names without loading any traffic or depending on C# at runtime.
        required = {
            "RawPackets": NAMES + ["Id", "IsProcessed", "Classification", "ClassifiedRecordId"],
            "Benign_Table": NAMES + ["Id", "ConfidenceScore", "ModelVersion"],
            "Attack_Table": NAMES + ["Id", "ConfidenceScore", "ModelVersion", "AttackType", "AttackCategory", "Severity", "IsAcknowledged"],
            "PipelineImports": ["FileHash", "Fingerprint", "Status", "LastRow", "Attempts", "NextAttempt"],
            "PipelineRows": ["FileHash", "RowNumber", "RawId", "State", "GateScore", "TypeScore", "ResultId"],
        }
        for table, names in required.items():
            self.conn.execute(f"SELECT {','.join('[' + name + ']' for name in names)} FROM {self.table(table)} WHERE 1=0")
        self.conn.commit()

    def fetch(self, sql, params=()):
        cursor = self.conn.execute(sql, params)
        names = [item[0] for item in cursor.description]
        rows = [dict(zip(names, row)) for row in cursor.fetchall()]
        self.conn.commit()
        return rows

    def job(self, digest):
        rows = self.fetch(f"SELECT * FROM {self.table('PipelineImports')} WHERE FileHash=?", (digest,))
        return rows[0] if rows else None

    def register(self, digest, filename, models):
        job = self.job(digest)
        if job is None:
            with self.transaction():
                self.conn.execute(f"""INSERT INTO {self.table('PipelineImports')}
                    (FileHash,FileName,Release,Fingerprint,GateVersion,TypeVersion,Status,CreatedAt,UpdatedAt)
                    VALUES (?,?,?,?,?,?,'staging',?,?)""",
                    (digest, filename, models.release, models.fingerprint, models.gate_version,
                     models.type_version, utcnow(), utcnow()))
            job = self.job(digest)
        return job

    def update(self, digest, **changes):
        allowed = {"Status", "Attempts", "NextAttempt", "LastError", "UpdatedAt"}
        if not changes.keys() <= allowed:
            raise ValueError("Unsupported job update")
        changes["UpdatedAt"] = utcnow()
        with self.transaction():
            self.conn.execute(f"UPDATE {self.table('PipelineImports')} SET "
                              + ",".join(f"[{key}]=?" for key in changes) + " WHERE FileHash=?",
                              (*changes.values(), digest))

    def insert_flow(self, table, features, **extras):
        data = {name: features[name] for name in NAMES}
        data.update(extras)
        names = ",".join(f"[{name}]" for name in data)
        values = list(data.values())
        if self.sqlite:
            values = [v.isoformat() if isinstance(v, datetime) else v for v in values]
        output = "" if self.sqlite else " OUTPUT INSERTED.Id"
        cursor = self.conn.execute(f"INSERT INTO {self.table(table)} ({names}){output} VALUES "
                                   f"({','.join('?' for _ in data)})", values)
        return cursor.lastrowid if self.sqlite else cursor.fetchone()[0]

    def stage(self, digest, rows):
        """Commit raw rows, errors and CSV checkpoint together, once per batch."""
        with self.transaction():
            for number, features, error in rows:
                raw_id = self.insert_flow("RawPackets", features, IsProcessed=0) if features else None
                self.conn.execute(f"""INSERT INTO {self.table('PipelineRows')}
                    (FileHash,RowNumber,RawId,State,Error) VALUES (?,?,?,?,?)""",
                    (digest, number, raw_id, "pending" if features else "rejected", error))
            self.conn.execute(f"UPDATE {self.table('PipelineImports')} SET LastRow=?, UpdatedAt=? WHERE FileHash=?",
                              (rows[-1][0], utcnow(), digest))

    def pending(self, digest, size):
        prefix = "" if self.sqlite else f"TOP ({int(size)}) "
        suffix = f" LIMIT {int(size)}" if self.sqlite else ""
        rows = self.fetch(f"""SELECT {prefix}p.RowNumber, p.RawId, {','.join('r.[' + name + ']' for name in NAMES)}
            FROM {self.table('PipelineRows')} p JOIN {self.table('RawPackets')} r ON r.Id=p.RawId
            WHERE p.FileHash=? AND p.State='pending' ORDER BY p.RowNumber{suffix}""", (digest,))
        for row in rows:
            if isinstance(row["Timestamp"], str):
                row["Timestamp"] = datetime.fromisoformat(row["Timestamp"])
        return rows

    def complete_batch(self, digest, rows, predictions, release):
        if len(rows) != len(predictions):
            raise ValueError("Prediction count does not match batch")
        with self.transaction():
            for row, prediction in zip(rows, predictions, strict=True):
                classification = "Attack" if prediction.attack else "Benign"
                # Raw Label preserves source truth; classified Label always reflects prediction.
                features = {**row, "Label": prediction.attack_type if prediction.attack else "Benign"}
                extras = dict(ConfidenceScore=prediction.type_score if prediction.attack else 1-prediction.gate_score,
                              ModelVersion=release)
                if prediction.attack:
                    extras.update(AttackType=prediction.attack_type, AttackCategory=prediction.category,
                                  Severity=prediction.severity, IsAcknowledged=0)
                result_id = self.insert_flow("Attack_Table" if prediction.attack else "Benign_Table", features, **extras)
                self.conn.execute(f"""UPDATE {self.table('RawPackets')}
                    SET IsProcessed=1, Classification=?, ClassifiedRecordId=? WHERE Id=?""",
                    (classification, result_id, row["RawId"]))
                self.conn.execute(f"""UPDATE {self.table('PipelineRows')}
                    SET State='completed',GateScore=?,TypeScore=?,ResultId=?,ProcessedAt=?
                    WHERE FileHash=? AND RowNumber=? AND State='pending'""",
                    (prediction.gate_score, prediction.type_score, result_id, utcnow(), digest, row["RowNumber"]))

    def counts(self, digest):
        rows = self.fetch(f"SELECT State,COUNT(*) AS Total FROM {self.table('PipelineRows')} WHERE FileHash=? GROUP BY State", (digest,))
        return {row["State"]: row["Total"] for row in rows}
