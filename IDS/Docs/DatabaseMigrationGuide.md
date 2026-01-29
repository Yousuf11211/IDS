# Database Migration Guide for Live Detection Tables
# =================================================

## Overview
This guide explains how to add the Benign_Table and Attack_Table to your database
for the live detection feature.

## Option 1: Using Entity Framework Migrations (Recommended)

### Step 1: Open Package Manager Console in Visual Studio
Tools -> NuGet Package Manager -> Package Manager Console

### Step 2: Add a new migration
```powershell
Add-Migration AddDetectionTables
```

### Step 3: Apply the migration
```powershell
Update-Database
```

## Option 2: Manual SQL Script

If you prefer to create the tables manually or need to add them to an existing database:

```sql
-- Benign Traffic Table
CREATE TABLE [dbo].[Benign_Table] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [DetectedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [SourceIP] NVARCHAR(45) NOT NULL,
    [DestinationIP] NVARCHAR(45) NOT NULL,
    [SourcePort] INT NOT NULL DEFAULT 0,
    [DestinationPort] INT NOT NULL DEFAULT 0,
    [Protocol] NVARCHAR(20) NOT NULL,
    [PacketSize] INT NOT NULL DEFAULT 0,
    [Duration] FLOAT NOT NULL DEFAULT 0,
    [BytesSent] BIGINT NOT NULL DEFAULT 0,
    [BytesReceived] BIGINT NOT NULL DEFAULT 0,
    [Service] NVARCHAR(50) NULL,
    [ConfidenceScore] FLOAT NOT NULL DEFAULT 0,
    [FeatureVector] NVARCHAR(4000) NULL,
 [ModelVersion] NVARCHAR(50) NULL
);

-- Indexes for Benign_Table
CREATE INDEX [IX_Benign_Table_DetectedAt] ON [dbo].[Benign_Table] ([DetectedAt]);
CREATE INDEX [IX_Benign_Table_SourceIP] ON [dbo].[Benign_Table] ([SourceIP]);
CREATE INDEX [IX_Benign_Table_DestinationIP] ON [dbo].[Benign_Table] ([DestinationIP]);
CREATE INDEX [IX_Benign_Table_Protocol] ON [dbo].[Benign_Table] ([Protocol]);

-- Attack Traffic Table
CREATE TABLE [dbo].[Attack_Table] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [DetectedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [SourceIP] NVARCHAR(45) NOT NULL,
    [DestinationIP] NVARCHAR(45) NOT NULL,
    [SourcePort] INT NOT NULL DEFAULT 0,
  [DestinationPort] INT NOT NULL DEFAULT 0,
    [Protocol] NVARCHAR(20) NOT NULL,
    [PacketSize] INT NOT NULL DEFAULT 0,
    [Duration] FLOAT NOT NULL DEFAULT 0,
    [BytesSent] BIGINT NOT NULL DEFAULT 0,
    [BytesReceived] BIGINT NOT NULL DEFAULT 0,
    [Service] NVARCHAR(50) NULL,
  [AttackType] NVARCHAR(100) NOT NULL,
    [AttackCategory] NVARCHAR(50) NULL,
    [Severity] NVARCHAR(20) NOT NULL DEFAULT 'Medium',
    [ConfidenceScore] FLOAT NOT NULL DEFAULT 0,
    [FeatureVector] NVARCHAR(4000) NULL,
    [ModelVersion] NVARCHAR(50) NULL,
    [IsAcknowledged] BIT NOT NULL DEFAULT 0,
    [AcknowledgedBy] NVARCHAR(450) NULL,
    [AcknowledgedAt] DATETIME2 NULL,
    [Notes] NVARCHAR(2000) NULL
);

-- Indexes for Attack_Table
CREATE INDEX [IX_Attack_Table_DetectedAt] ON [dbo].[Attack_Table] ([DetectedAt]);
CREATE INDEX [IX_Attack_Table_SourceIP] ON [dbo].[Attack_Table] ([SourceIP]);
CREATE INDEX [IX_Attack_Table_DestinationIP] ON [dbo].[Attack_Table] ([DestinationIP]);
CREATE INDEX [IX_Attack_Table_AttackType] ON [dbo].[Attack_Table] ([AttackType]);
CREATE INDEX [IX_Attack_Table_Severity] ON [dbo].[Attack_Table] ([Severity]);
CREATE INDEX [IX_Attack_Table_IsAcknowledged] ON [dbo].[Attack_Table] ([IsAcknowledged]);
```

## Option 3: If Your Detection Pipeline Creates the Tables

If your detection pipeline (Python/ML model) creates these tables directly, ensure:

1. The table names match: `Benign_Table` and `Attack_Table`
2. The column names and types match the schema above
3. The `Id` column is an auto-incrementing primary key

The IDS web application will then read from these tables and broadcast updates
to connected dashboard clients in real-time.

## Architecture Overview

```
???????????????????????? ???????????????????   ???????????????????
?  Detection Pipeline  ?????>?    Database     ?<?????   IDS Web App   ?
?  (Python/ML Model)   ??  Benign_Table   ?     ? SignalR Hub     ?
? ?     ?  Attack_Table   ?  ?          ?
????????????????????????     ???????????????????     ???????????????????
         ?
   ?????????????????????
  ?  Connected Users  ?
  ?  (Dashboard)      ?
           ?????????????????????
```

### How It Works:

1. **Detection Pipeline** writes to `Benign_Table` or `Attack_Table`
2. **DetectionMonitorService** (background service) polls for new records every 2 seconds
3. When new records are found, it broadcasts to all connected clients via **SignalR**
4. **Live Dashboard** receives updates in real-time and displays them

### Alternative: Direct API Push

Instead of polling, your pipeline can push data directly via the API:

```
????????????????????????     ???????????????????     ???????????????????
?  Detection Pipeline  ?????>?   IDS Web App   ?????>?  Connected Users?
?  (Python/ML Model)   ?     ?   /api/detection?     ?  (Dashboard)    ?
????????????????????????     ???????????????????     ???????????????????
```

Use the API endpoints:
- `POST /api/detection/benign` - Single benign traffic
- `POST /api/detection/attack` - Single attack
- `POST /api/detection/batch` - Multiple detections

See `Examples/detection_pipeline_example.py` for integration examples.

## Configuration

In `appsettings.json`:
```json
{
  "Detection": {
    "ApiKey": "your-secure-api-key",
    "PollingIntervalSeconds": 2,
    "StatsUpdateIntervalSeconds": 10
  }
}
```

Or via environment variables:
```
DETECTION_API_KEY=your-secure-api-key
```
