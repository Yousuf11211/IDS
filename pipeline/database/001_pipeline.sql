-- Apply to the SAME SQL Server database as IDS after its EF migrations.
-- Owns only pipeline metadata; never alters the web application's tables.
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.PipelineImports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PipelineImports (
        FileHash varchar(64) NOT NULL PRIMARY KEY,
        FileName nvarchar(512) NOT NULL,
        Release nvarchar(50) NOT NULL,
        Fingerprint varchar(64) NOT NULL,
        GateVersion nvarchar(100) NOT NULL,
        TypeVersion nvarchar(100) NOT NULL,
        Status varchar(32) NOT NULL,
        LastRow bigint NOT NULL DEFAULT 0,
        Attempts int NOT NULL DEFAULT 0,
        NextAttempt float NOT NULL DEFAULT 0,
        LastError nvarchar(1000) NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL
    );
END;
IF OBJECT_ID(N'dbo.PipelineRows', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PipelineRows (
        FileHash varchar(64) NOT NULL,
        RowNumber bigint NOT NULL,
        RawId bigint NULL,
        State varchar(20) NOT NULL,
        Error nvarchar(1000) NULL,
        GateScore float NULL,
        TypeScore float NULL,
        ResultId bigint NULL,
        ProcessedAt datetime2 NULL,
        CONSTRAINT PK_PipelineRows PRIMARY KEY (FileHash, RowNumber),
        CONSTRAINT FK_PipelineRows_Import FOREIGN KEY (FileHash)
            REFERENCES dbo.PipelineImports(FileHash),
        CONSTRAINT FK_PipelineRows_Raw FOREIGN KEY (RawId)
            REFERENCES dbo.RawPackets(Id)
    );
    CREATE INDEX IX_PipelineRows_Pending ON dbo.PipelineRows(FileHash, State, RowNumber);
    CREATE UNIQUE INDEX IX_PipelineRows_RawId ON dbo.PipelineRows(RawId) WHERE RawId IS NOT NULL;
END;
