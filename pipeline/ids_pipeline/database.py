"""Transactional persistence in the same SQL Server database as the IDS web app."""
from contextlib import contextmanager
from datetime import datetime, timezone
import json
import logging
import time

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

    def write_heartbeat(self, worker, running):
        with self.transaction():
            if running:
                self.conn.execute("""
                    UPDATE dbo.WorkerHeartbeats SET LastHeartbeatUtc=SYSUTCDATETIME(), IsRunning=1 WHERE Worker=?;
                    IF @@ROWCOUNT=0
                        INSERT INTO dbo.WorkerHeartbeats (Worker, LastHeartbeatUtc, IsRunning)
                        VALUES (?, SYSUTCDATETIME(), 1);
                """, worker, worker)
            else:
                self.conn.execute("UPDATE dbo.WorkerHeartbeats SET IsRunning=0 WHERE Worker=?", worker)

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
            "FlowExplanations": ["Id", "AttackTrafficId", "RawPacketId", "Stage", "ModelVersion",
                                 "Fingerprint", "ContextJson", "Status", "PayloadJson"],
        }
        subflow_columns = self.conn.execute("""
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME IN ('RawPackets','Benign_Table','Attack_Table')
              AND COLUMN_NAME IN ('SubflowFwdPackets','SubflowBwdPackets','SubflowFwdBytes','SubflowBwdBytes')
              AND DATA_TYPE='float' AND NUMERIC_PRECISION=53
        """).fetchone()[0]
        if subflow_columns != 12:
            raise RuntimeError("The three flow tables require float(53) subflow columns. "
                               "Apply the fractional subflow migration before running the workers.")
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

    def complete_batch(self, digest, rows, predictions, release, models=None):
        if len(rows) != len(predictions):
            raise ValueError("Prediction count does not match batch")
        with self.transaction():
            enqueue = models is not None and any(p.attack for p in predictions) and self.shap_generation_enabled()
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
                if prediction.attack and enqueue:
                    self.enqueue_explanations(result_id, row["RawId"], row, prediction, models)

    def shap_generation_enabled(self):
        setting = self.conn.execute(
            "SELECT Value FROM dbo.SystemSettings WHERE [Key]=?", "Operations.ShapJobGenerationEnabled"
        ).fetchone()
        return setting is None or setting[0].strip().lower() == "true"

    def enqueue_explanations(self, attack_id, raw_id, row, prediction, models):
        """Called in detection/backfill transaction; only freezes inputs, never runs SHAP."""
        for stage, version in (("gatekeeper", models.gate_version), ("multiclass", models.type_version)):
            context = models.explanation_context(stage, row, prediction)
            context.update(attackId=str(attack_id), rawId=str(raw_id))
            self.conn.execute("""INSERT INTO dbo.FlowExplanations
                (AttackTrafficId,RawPacketId,Stage,ModelVersion,Fingerprint,ContextJson,Status,
                 Attempts,NextAttempt,CreatedAt,UpdatedAt)
                SELECT ?,?,?,?,?,?,'pending',0,0,?,?
                WHERE NOT EXISTS (SELECT 1 FROM dbo.FlowExplanations WITH (UPDLOCK,HOLDLOCK)
                                  WHERE AttackTrafficId=? AND Stage=? AND ModelVersion=?)""",
                (attack_id, raw_id, stage, version, models.fingerprint,
                 json.dumps(context, allow_nan=False), utcnow(), utcnow(), attack_id, stage, version))

    def acquire_explanation_lock(self):
        result = self.conn.execute("""SET NOCOUNT ON; DECLARE @result int;
            EXEC @result=sys.sp_getapplock @Resource='IDS.SHAP.v1',@LockMode='Exclusive',
                @LockOwner='Session',@LockTimeout=0; SELECT @result;""").fetchone()[0]
        self.conn.commit()
        if result < 0:
            raise RuntimeError("Another explanation worker owns this database")
        # Exclusive session ownership proves the previous worker no longer owns a lease.
        with self.transaction():
            self.conn.execute("UPDATE dbo.FlowExplanations SET Status='pending',UpdatedAt=? WHERE Status='processing'",
                              (utcnow(),))

    def claim_explanation(self, fingerprint):
        with self.transaction():
            cursor = self.conn.execute(""";WITH next_job AS (
                SELECT TOP(1) * FROM dbo.FlowExplanations WITH (UPDLOCK,READPAST,ROWLOCK)
                WHERE Status='pending' AND Fingerprint=? AND NextAttempt<=?
                ORDER BY Id)
                UPDATE next_job SET Status='processing',Attempts=Attempts+1,UpdatedAt=? OUTPUT INSERTED.*""",
                (fingerprint, time.time(), utcnow()))
            names = [item[0] for item in cursor.description]
            row = cursor.fetchone()
            return dict(zip(names, row)) if row else None

    def finish_explanation(self, explanation_id, payload):
        with self.transaction():
            self.conn.execute("""UPDATE dbo.FlowExplanations SET Status='completed',PayloadJson=?,
                LastError=NULL,UpdatedAt=? WHERE Id=? AND Status='processing'""",
                (json.dumps(payload, allow_nan=False), utcnow(), explanation_id))

    def fail_explanation(self, explanation_id, error):
        # Do not persist SQL driver exceptions/connection strings in a browser-readable field.
        message = f"{type(error).__name__}: " + (str(error) if isinstance(error,
                    (ValueError, TypeError, ImportError, AssertionError)) else "SHAP generation failed; consult worker logs")
        logging.getLogger(__name__).error("Explanation %s failed: %s", explanation_id, message[:1000])
        with self.transaction():
            self.conn.execute("""UPDATE dbo.FlowExplanations SET
                Status=CASE WHEN Attempts>=? THEN 'failed' ELSE 'pending' END,
                LastError=?,NextAttempt=?,UpdatedAt=? WHERE Id=? AND Status='processing'""",
                (self.config.max_attempts, message[:1000], time.time()+self.config.retry_seconds,
                 utcnow(), explanation_id))

    def retry_explanations(self, fingerprint, attack_id):
        with self.transaction():
            self.conn.execute("""UPDATE dbo.FlowExplanations SET Status='pending',Attempts=0,
                NextAttempt=0,LastError=NULL,UpdatedAt=?
                WHERE Fingerprint=? AND AttackTrafficId=? AND Status='failed'""",
                (utcnow(), fingerprint, attack_id))

    def backfill_explanations(self, models, limit=100):
        """Only original pipeline rows whose exact release and original score can be verified."""
        fields = ','.join(f'r.[{name}] AS [{SQL_TO_CSV[name]}]' for name in NAMES)
        rows = self.fetch(f"""SELECT TOP ({int(limit)}) a.Id AS AttackId,p.RawId,p.GateScore,p.TypeScore,
            a.AttackType,a.ConfidenceScore,{fields}
            FROM dbo.PipelineRows p JOIN dbo.PipelineImports i ON i.FileHash=p.FileHash
            JOIN dbo.RawPackets r ON r.Id=p.RawId
            JOIN dbo.Attack_Table a ON a.Id=p.ResultId AND a.ModelVersion=i.Release
            WHERE p.State='completed' AND r.Classification='Attack' AND r.ClassifiedRecordId=a.Id
                AND i.Fingerprint=? AND NOT EXISTS
                (SELECT 1 FROM dbo.FlowExplanations e WHERE e.AttackTrafficId=a.Id)
            ORDER BY a.Id""", (models.fingerprint,))
        count = 0
        for row in rows:
            prediction = models.predict([row])[0]
            if (not prediction.attack or prediction.attack_type != row["AttackType"] or
                    row["GateScore"] is None or row["TypeScore"] is None or
                    abs(prediction.gate_score-row["GateScore"]) > 1e-6 or
                    abs(prediction.type_score-row["TypeScore"]) > 1e-6 or
                    abs(prediction.type_score-row["ConfidenceScore"]) > 1e-6):
                logging.getLogger(__name__).warning("Cannot reproduce historical attack %s", row["AttackId"])
                continue
            with self.transaction():
                self.enqueue_explanations(row["AttackId"], row["RawId"], row, prediction, models)
            count += 1
        return count

    def counts(self, digest):
        rows = self.fetch(f"SELECT State,COUNT(*) AS Total FROM {self.table('PipelineRows')} WHERE FileHash=? GROUP BY State", (digest,))
        return {row["State"]: row["Total"] for row in rows}
