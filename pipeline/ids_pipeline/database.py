"""Transactional persistence in the same SQL Server database as the IDS web app."""
from contextlib import contextmanager
from datetime import datetime, timezone

from .connection import connection_string
from .schema import NAMES, SQL_TO_CSV


def utcnow():
    # SQL Server datetime2 accepts the ISO-8601 representation.
    return datetime.now(timezone.utc).replace(tzinfo=None).isoformat()


class Store:
    def __init__(self, config):
        self.config = config
        try:
            import pyodbc
        except ImportError:
            raise RuntimeError("Install the pipeline dependencies and Microsoft ODBC Driver 18 for SQL Server. "
                               "On Linux, unixODBC is also required.") from None
        installed = pyodbc.drivers()
        driver = config.odbc_driver
        if driver == "auto":
            driver = next((name for name in ("ODBC Driver 18 for SQL Server", "ODBC Driver 17 for SQL Server")
                           if name in installed), None)
        if not driver or driver not in installed:
            raise RuntimeError("Install Microsoft ODBC Driver 18 or 17 for this Python interpreter's platform/architecture")
        pyodbc.pooling = False  # Closing the session must release its application lock.
        self.conn = pyodbc.connect(connection_string(config, driver), autocommit=False, timeout=15)
        self.conn.timeout = 60
        self.conn.execute("SET XACT_ABORT ON")
        self.conn.commit()

    def table(self, name):
        return f"[dbo].[{name}]"

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

    def acquire_lock(self):
        """One worker per database. A disconnected worker must stop and reacquire."""
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
        with self.transaction():
            self.conn.execute((self.config.root / "database/001_pipeline.sql").read_text())

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
        data = {name: features[SQL_TO_CSV[name]] for name in NAMES}
        data.update(extras)
        names = ",".join(f"[{name}]" for name in data)
        values = list(data.values())
        cursor = self.conn.execute(f"INSERT INTO {self.table(table)} ({names}) OUTPUT INSERTED.Id VALUES "
                                   f"({','.join('?' for _ in data)})", values)
        return cursor.fetchone()[0]

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
        fields = ','.join(f'r.[{name}] AS [{SQL_TO_CSV[name]}]' for name in NAMES)
        return self.fetch(f"""SELECT TOP ({int(size)}) p.RowNumber, p.RawId, {fields}
            FROM {self.table('PipelineRows')} p JOIN {self.table('RawPackets')} r ON r.Id=p.RawId
            WHERE p.FileHash=? AND p.State='pending' ORDER BY p.RowNumber""", (digest,))

    def complete_batch(self, digest, rows, predictions, release):
        if len(rows) != len(predictions):
            raise ValueError("Prediction count does not match batch")
        with self.transaction():
            for row, prediction in zip(rows, predictions, strict=True):
                classification = "Attack" if prediction.attack else "Benign"
                # Raw Label preserves source truth; classified Label always reflects prediction.
                features = {**row, "label": prediction.attack_type if prediction.attack else "Benign"}
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
