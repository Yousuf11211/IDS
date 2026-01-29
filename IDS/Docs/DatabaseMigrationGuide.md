# Database Setup Guide for Live Detection Tables
# ==============================================

## Overview

This IDS web application **reads** from two database tables that are **written to by your external detection pipeline**:

- `Benign_Table` - Stores normal/safe network traffic
- `Attack_Table` - Stores detected attacks/malicious traffic

```
????????????????????????????     ???????????????????       ?????????????????????
?   Detection Pipeline     ???????>?  Database     ?<???????   IDS Web App   ?
? (Python / ML Model)    ? WRITE ?  Benign_Table   ? READ  ?   (Dashboard)     ?
?  ?    ?  Attack_Table   ?       ?           ?
????????????????????????????       ???????????????????       ?????????????????????
   ? SignalR
  ???????????????????
     ?  Browser Users  ?
       ?  (Live Updates) ?
         ???????????????????
```

## How It Works

1. Your **detection pipeline** (separate Python/ML application) analyzes network traffic
2. Pipeline writes classification results to `Benign_Table` or `Attack_Table`
3. **DetectionMonitorService** (background service in web app) polls tables every 2 seconds
4. When new records are found, they are broadcast to all connected dashboards via **SignalR**
5. Users see live updates on the **Live Dashboard** page

## Database Table Schema

### Option 1: Let Entity Framework Create Tables

Run these commands in Visual Studio Package Manager Console:

```powershell
Add-Migration AddDetectionTables
Update-Database
```

### Option 2: Create Tables Manually (SQL Server)

If your pipeline creates the tables, use this schema:

```sql
-- =====================================================
-- Benign Traffic Table
-- Stores normal/safe network traffic detected by ML model
-- =====================================================
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

-- Indexes for better query performance
CREATE INDEX [IX_Benign_Table_DetectedAt] ON [dbo].[Benign_Table] ([DetectedAt] DESC);
CREATE INDEX [IX_Benign_Table_SourceIP] ON [dbo].[Benign_Table] ([SourceIP]);


-- =====================================================
-- Attack Traffic Table  
-- Stores detected attacks/malicious traffic
-- =====================================================
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

-- Indexes for better query performance
CREATE INDEX [IX_Attack_Table_DetectedAt] ON [dbo].[Attack_Table] ([DetectedAt] DESC);
CREATE INDEX [IX_Attack_Table_SourceIP] ON [dbo].[Attack_Table] ([SourceIP]);
CREATE INDEX [IX_Attack_Table_AttackType] ON [dbo].[Attack_Table] ([AttackType]);
CREATE INDEX [IX_Attack_Table_Severity] ON [dbo].[Attack_Table] ([Severity]);
CREATE INDEX [IX_Attack_Table_IsAcknowledged] ON [dbo].[Attack_Table] ([IsAcknowledged]);
```

## Required Columns

### Benign_Table (Minimum Required)

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| Id | BIGINT | Yes | Auto-increment primary key |
| DetectedAt | DATETIME2 | Yes | When the traffic was detected |
| SourceIP | NVARCHAR(45) | Yes | Source IP address |
| DestinationIP | NVARCHAR(45) | Yes | Destination IP address |
| Protocol | NVARCHAR(20) | Yes | Network protocol (TCP, UDP, etc.) |
| ConfidenceScore | FLOAT | No | ML model confidence (0.0 - 1.0) |

### Attack_Table (Minimum Required)

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| Id | BIGINT | Yes | Auto-increment primary key |
| DetectedAt | DATETIME2 | Yes | When the attack was detected |
| SourceIP | NVARCHAR(45) | Yes | Attacker IP address |
| DestinationIP | NVARCHAR(45) | Yes | Target IP address |
| Protocol | NVARCHAR(20) | Yes | Network protocol |
| AttackType | NVARCHAR(100) | Yes | Type of attack (DoS, DDoS, PortScan, etc.) |
| Severity | NVARCHAR(20) | Yes | Severity level (Critical, High, Medium, Low) |
| ConfidenceScore | FLOAT | No | ML model confidence (0.0 - 1.0) |


## Python Pipeline Example

Here's how your Python detection pipeline can write to these tables:

```python
import pyodbc
from datetime import datetime

# Database connection
conn_str = (
    "Driver={ODBC Driver 17 for SQL Server};"
    "Server=localhost;"
    "Database=IDS;"
 "Trusted_Connection=yes;"
)
conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

def save_benign_traffic(src_ip, dst_ip, protocol, confidence):
    """Save benign traffic detection to database."""
    cursor.execute("""
        INSERT INTO Benign_Table 
        (DetectedAt, SourceIP, DestinationIP, Protocol, ConfidenceScore)
   VALUES (?, ?, ?, ?, ?)
    """, datetime.utcnow(), src_ip, dst_ip, protocol, confidence)
    conn.commit()

def save_attack(src_ip, dst_ip, protocol, attack_type, severity, confidence):
    """Save attack detection to database."""
    cursor.execute("""
        INSERT INTO Attack_Table 
        (DetectedAt, SourceIP, DestinationIP, Protocol, AttackType, Severity, ConfidenceScore)
        VALUES (?, ?, ?, ?, ?, ?, ?)
    """, datetime.utcnow(), src_ip, dst_ip, protocol, attack_type, severity, confidence)
    conn.commit()

# Example usage in your ML pipeline:
def process_packet(packet_features, model_prediction):
    if model_prediction['is_attack']:
     save_attack(
            src_ip=packet_features['src_ip'],
            dst_ip=packet_features['dst_ip'],
            protocol=packet_features['protocol'],
            attack_type=model_prediction['attack_type'],
       severity=model_prediction['severity'],
   confidence=model_prediction['confidence']
        )
    else:
        save_benign_traffic(
     src_ip=packet_features['src_ip'],
        dst_ip=packet_features['dst_ip'],
        protocol=packet_features['protocol'],
   confidence=model_prediction['confidence']
        )
```

## Configuration

In `appsettings.json`, configure the polling intervals:

```json
{
"Detection": {
    "PollingIntervalSeconds": 2,
    "StatsUpdateIntervalSeconds": 10
  }
}
```

- **PollingIntervalSeconds**: How often to check for new records (default: 2 seconds)
- **StatsUpdateIntervalSeconds**: How often to broadcast updated statistics (default: 10 seconds)

## Testing

1. Start the IDS web application
2. Log in and navigate to `/LiveDashboard`
3. Manually insert test data into the tables:

```sql
-- Test benign traffic
INSERT INTO Benign_Table (SourceIP, DestinationIP, Protocol, ConfidenceScore)
VALUES ('192.168.1.100', '10.0.0.5', 'TCP', 0.95);

-- Test attack
INSERT INTO Attack_Table (SourceIP, DestinationIP, Protocol, AttackType, Severity, ConfidenceScore)
VALUES ('192.168.1.200', '10.0.0.5', 'TCP', 'DoS', 'High', 0.92);
```

4. Within 2 seconds, you should see the new data appear on the Live Dashboard!
