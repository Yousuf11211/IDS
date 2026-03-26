# =============================================================================
# IDS Detection Pipeline - Database Writer Example
# =============================================================================
#
# This script shows how your detection pipeline can write directly to the 
# database tables. The IDS web application will automatically detect new 
# records and update connected dashboards in real-time.
#
# Tables (all 172 network flow features):
#   - RawPackets: Stores all captured packets (before classification)
#   - Benign_Table: Stores benign/normal traffic (after classification)
#   - Attack_Table: Stores detected attacks (after classification)
#
# Requirements:
#   pip install pyodbc
#
# =============================================================================

import pyodbc
from datetime import datetime
from typing import Optional, Dict, List
import json


# All 172 feature columns in order (matching the C# NetworkFlowBase)
FEATURE_COLUMNS = [
  "Timestamp", "SrcIp", "DstPort", "Duration", "PacketsCount",
    "FwdPacketsCount", "BwdPacketsCount", "TotalPayloadBytes", "FwdTotalPayloadBytes", "BwdTotalPayloadBytes",
    "PayloadBytesMax", "PayloadBytesMean", "PayloadBytesStd", "PayloadBytesVariance", "PayloadBytesMedian",
    "PayloadBytesSkewness", "PayloadBytesCov", "FwdPayloadBytesMax", "FwdPayloadBytesStd", "FwdPayloadBytesMedian",
    "FwdPayloadBytesSkewness", "FwdPayloadBytesCov", "BwdPayloadBytesMax", "BwdPayloadBytesStd", "BwdPayloadBytesMedian",
    "TotalHeaderBytes", "MeanHeaderBytes", "StdHeaderBytes", "SkewnessHeaderBytes", "VarianceHeaderBytes",
    "FwdTotalHeaderBytes", "FwdMeanHeaderBytes", "BwdTotalHeaderBytes", "BwdMeanHeaderBytes", "FwdAvgSegmentSize",
    "AvgSegmentSize", "FwdInitWinBytes", "BwdInitWinBytes", "ActiveMin", "ActiveMax",
    "ActiveMean", "ActiveStd", "ActiveMedian", "ActiveSkewness", "ActiveCov",
    "ActiveMode", "ActiveVariance", "IdleMin", "IdleMax", "IdleMean",
    "IdleStd", "IdleMedian", "IdleSkewness", "IdleCov", "IdleMode",
    "IdleVariance", "BytesRate", "FwdBytesRate", "BwdBytesRate", "PacketsRate",
    "BwdPacketsRate", "FwdPacketsRate", "DownUpRate", "AvgBwdBulkRate", "BwdBulkStateCount",
    "BwdBulkDuration", "FinFlagCounts", "PshFlagCounts", "UrgFlagCounts", "EceFlagCounts",
    "SynFlagCounts", "AckFlagCounts", "CwrFlagCounts", "RstFlagCounts", "FwdFinFlagCounts",
    "FwdPshFlagCounts", "FwdUrgFlagCounts", "FwdSynFlagCounts", "FwdAckFlagCounts", "FwdRstFlagCounts",
 "BwdFinFlagCounts", "BwdUrgFlagCounts", "BwdSynFlagCounts", "BwdAckFlagCounts", "BwdRstFlagCounts",
    "SynFlagPercentageInTotal", "AckFlagPercentageInTotal", "RstFlagPercentageInTotal", "FwdAckFlagPercentageInTotal", "BwdAckFlagPercentageInTotal",
    "FwdAckFlagPercentageInFwdPackets", "BwdAckFlagPercentageInBwdPackets", "PacketsIatMean", "PacketIatStd", "PacketIatMax",
    "PacketIatMin", "PacketIatTotal", "PacketsIatMedian", "PacketsIatCov", "PacketsIatMode",
    "PacketsIatVariance", "FwdPacketsIatMean", "FwdPacketsIatStd", "FwdPacketsIatMax", "FwdPacketsIatMin",
    "FwdPacketsIatTotal", "FwdPacketsIatMedian", "FwdPacketsIatSkewness", "FwdPacketsIatCov", "FwdPacketsIatMode",
    "FwdPacketsIatVariance", "BwdPacketsIatMean", "BwdPacketsIatStd", "BwdPacketsIatMax", "BwdPacketsIatMin",
    "BwdPacketsIatTotal", "BwdPacketsIatMedian", "BwdPacketsIatMode", "BwdPacketsIatVariance", "SubflowFwdPackets",
    "SubflowBwdPackets", "SubflowFwdBytes", "SubflowBwdBytes", "DeltaStart", "HandshakeDuration",
    "HandshakeState", "MaxBwdPacketsDeltaTime", "MeanPacketsDeltaTime", "ModePacketsDeltaTime", "VariancePacketsDeltaTime",
    "StdPacketsDeltaTime", "SkewnessPacketsDeltaTime", "MeanBwdPacketsDeltaTime", "SkewnessBwdPacketsDeltaTime", "CovBwdPacketsDeltaTime",
    "MinFwdPacketsDeltaTime", "MaxFwdPacketsDeltaTime", "MeanFwdPacketsDeltaTime", "ModeFwdPacketsDeltaTime", "StdFwdPacketsDeltaTime",
    "MedianFwdPacketsDeltaTime", "SkewnessFwdPacketsDeltaTime", "CovFwdPacketsDeltaTime", "MeanPacketsDeltaLen", "ModePacketsDeltaLen",
    "MedianPacketsDeltaLen", "SkewnessPacketsDeltaLen", "CovPacketsDeltaLen", "MaxBwdPacketsDeltaLen", "MeanBwdPacketsDeltaLen",
    "ModeBwdPacketsDeltaLen", "MedianBwdPacketsDeltaLen", "SkewnessBwdPacketsDeltaLen", "CovBwdPacketsDeltaLen", "MeanFwdPacketsDeltaLen",
    "ModeFwdPacketsDeltaLen", "MedianFwdPacketsDeltaLen", "SkewnessFwdPacketsDeltaLen", "CovFwdPacketsDeltaLen", "MaxHeaderBytesDeltaLen",
    "MeanHeaderBytesDeltaLen", "CovHeaderBytesDeltaLen", "MeanBwdHeaderBytesDeltaLen", "CovBwdHeaderBytesDeltaLen", "MeanFwdHeaderBytesDeltaLen",
    "CovFwdHeaderBytesDeltaLen", "MinPayloadBytesDeltaLen", "MaxPayloadBytesDeltaLen", "MeanPayloadBytesDeltaLen", "ModePayloadBytesDeltaLen",
    "ModeFwdPayloadBytesDeltaLen", "Label"
]


class IDSDatabaseWriter:
    """
    Writes detection results to the IDS database.
    The web application monitors these tables and broadcasts updates via SignalR.
    """
    
    def __init__(self, connection_string: str):
        self.conn_str = connection_string
        self.conn = None
        self.cursor = None

def connect(self):
        self.conn = pyodbc.connect(self.conn_str)
  self.cursor = self.conn.cursor()
        print("[IDS] Connected to database")
   
  def disconnect(self):
  if self.cursor:
         self.cursor.close()
    if self.conn:
            self.conn.close()
        print("[IDS] Disconnected from database")
    
    def __enter__(self):
     self.connect()
  return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        self.disconnect()

    def save_raw_packet(self, features: Dict) -> int:
      """
    Save a raw packet with all 172 features to the database.
     
        Args:
      features: Dictionary with all network flow features            
        Returns:
            The ID of the inserted record.
        """
        columns = ", ".join(FEATURE_COLUMNS)
        placeholders = ", ".join(["?" for _ in FEATURE_COLUMNS])
   
        values = [features.get(col.lower(), features.get(col, self._default_value(col))) 
        for col in FEATURE_COLUMNS]
     
        self.cursor.execute(f"""
    INSERT INTO RawPackets ({columns}, IsProcessed)
            OUTPUT INSERTED.Id
     VALUES ({placeholders}, 0)
""", values)
        
        row = self.cursor.fetchone()
        self.conn.commit()
        return row[0] if row else 0

    def save_benign(self, features: Dict, confidence_score: float = 0.95, model_version: str = "v1.0") -> int:
        """
   Save benign traffic with all 172 features.
        """
  columns = ", ".join(FEATURE_COLUMNS)
        placeholders = ", ".join(["?" for _ in FEATURE_COLUMNS])
      
        values = [features.get(col.lower(), features.get(col, self._default_value(col))) 
    for col in FEATURE_COLUMNS]
  
        self.cursor.execute(f"""
    INSERT INTO Benign_Table ({columns}, ConfidenceScore, ModelVersion)
OUTPUT INSERTED.Id
 VALUES ({placeholders}, ?, ?)
   """, values + [confidence_score, model_version])
        
        row = self.cursor.fetchone()
        self.conn.commit()
  return row[0] if row else 0

    def save_attack(
      self, 
        features: Dict, 
        attack_type: str,
        severity: str = "Medium",
        attack_category: Optional[str] = None,
    confidence_score: float = 0.95, 
        model_version: str = "v1.0"
    ) -> int:
        """
        Save attack traffic with all 172 features.
  """
        columns = ", ".join(FEATURE_COLUMNS)
        placeholders = ", ".join(["?" for _ in FEATURE_COLUMNS])
        
     values = [features.get(col.lower(), features.get(col, self._default_value(col))) 
     for col in FEATURE_COLUMNS]
     
        self.cursor.execute(f"""
            INSERT INTO Attack_Table 
            ({columns}, AttackType, AttackCategory, Severity, ConfidenceScore, ModelVersion, IsAcknowledged)
  OUTPUT INSERTED.Id
VALUES ({placeholders}, ?, ?, ?, ?, ?, 0)
      """, values + [attack_type, attack_category, severity, confidence_score, model_version])
        
      row = self.cursor.fetchone()
    self.conn.commit()
        
        print(f"[IDS] Attack saved: {attack_type} ({severity}) from {features.get('SrcIp', features.get('src_ip', 'unknown'))}")
        return row[0] if row else 0

    def mark_packet_processed(self, packet_id: int, classification: str, classified_record_id: Optional[int] = None):
     """Mark a raw packet as processed after classification."""
        self.cursor.execute("""
            UPDATE RawPackets 
       SET IsProcessed = 1, Classification = ?, ClassifiedRecordId = ?
            WHERE Id = ?
        """, classification, classified_record_id, packet_id)
        self.conn.commit()

    def _default_value(self, column_name: str):
        """Return default value based on column type."""
        if column_name == "Timestamp":
   return datetime.utcnow()
        elif column_name in ["SrcIp", "HandshakeState", "Label"]:
          return ""
    else:
            return 0


def create_feature_dict_from_row(row: List, column_names: List[str]) -> Dict:
    """
    Create a feature dictionary from a data row and column names.
    Use this when reading from CSV or DataFrame.
    """
    return {col: val for col, val in zip(column_names, row)}


def determine_severity(attack_type: str, confidence: float) -> str:
    """Determine severity based on attack type and confidence score."""
    attack_type_lower = attack_type.lower()
    
    if attack_type_lower in ["dos", "ddos", "infiltration", "botnet", "ransomware"]:
 return "Critical" if confidence > 0.9 else "High"
    
 if attack_type_lower in ["portscan", "bruteforce", "sql_injection", "xss"]:
 return "High" if confidence > 0.8 else "Medium"
    
    if confidence > 0.7:
        return "Medium"

    return "Low"


# =============================================================================
# Example: Using with your ML Pipeline
# =============================================================================

def process_flow_prediction(writer: IDSDatabaseWriter, features: Dict, prediction: Dict):
    """
    Process a flow prediction from your ML model.
    
    Args:
        writer: IDSDatabaseWriter instance
        features: Dictionary with all 172 network flow features
        prediction: Dict with 'is_attack', 'attack_type', 'confidence', etc.
    """
    # Ensure timestamp is set
    if 'Timestamp' not in features and 'timestamp' not in features:
   features['Timestamp'] = datetime.utcnow()
    
    # Set label from prediction if not already set
    if 'Label' not in features and 'label' not in features:
        if prediction.get('is_attack'):
            features['Label'] = prediction.get('attack_type', 'Attack')
        else:
       features['Label'] = 'Benign'
    
    if prediction.get('is_attack', False):
        attack_type = prediction.get('attack_type', 'Unknown')
  confidence = prediction.get('confidence', 0.5)
   severity = prediction.get('severity') or determine_severity(attack_type, confidence)
        
        return writer.save_attack(
            features=features,
       attack_type=attack_type,
            severity=severity,
            attack_category=prediction.get('attack_category'),
   confidence_score=confidence,
            model_version=prediction.get('model_version', 'v1.0')
        )
    else:
 return writer.save_benign(
   features=features,
            confidence_score=prediction.get('confidence', 0.95),
            model_version=prediction.get('model_version', 'v1.0')
     )


# =============================================================================
# Example: Simulated Detection Pipeline (for testing)
# =============================================================================

def run_simulation():
    """Run a simulation that generates random detections for testing."""
    import random
    import time
    
    conn_str = (
        "Driver={ODBC Driver 17 for SQL Server};"
        "Server=localhost;"
        "Database=IDS;"

      "Trusted_Connection=yes;"
    )
    
    attack_types = [
        ("DoS", "DoS", "Critical"),
        ("DDoS", "DoS", "Critical"),
        ("PortScan", "Probe", "Medium"),
        ("BruteForce", "Probe", "High"),
        ("SQLInjection", "R2L", "High"),
        ("Infiltration", "U2R", "Critical"),
    ]
    
    print("Starting detection simulation with 172 features...")
    print("Press Ctrl+C to stop\n")
    
 with IDSDatabaseWriter(conn_str) as writer:
        count = 0
    while True:
       try:
        # Generate random features (simplified - your ML model would provide these)
    features = {
       'Timestamp': datetime.utcnow(),
    'SrcIp': f"192.168.{random.randint(1, 255)}.{random.randint(1, 255)}",
'DstPort': random.choice([80, 443, 22, 21, 25, 53, 3306]),
           'Duration': random.uniform(0.001, 5.0),
     'PacketsCount': random.randint(1, 1000),
       'FwdPacketsCount': random.randint(1, 500),
       'BwdPacketsCount': random.randint(0, 500),
      'TotalPayloadBytes': random.randint(100, 50000),
                 'FwdTotalPayloadBytes': random.randint(50, 25000),
          'BwdTotalPayloadBytes': random.randint(50, 25000),
 'PayloadBytesMax': random.uniform(0, 1500),
   'PayloadBytesMean': random.uniform(0, 500),
     'PayloadBytesStd': random.uniform(0, 200),
              'SynFlagCounts': random.randint(0, 10),
        'AckFlagCounts': random.randint(0, 100),
         'FinFlagCounts': random.randint(0, 5),
    'BytesRate': random.uniform(0, 10000),
         'PacketsRate': random.uniform(0, 1000),
        'Label': '',  # Will be set by process_flow_prediction
 # ... other features default to 0
     }
      
          # 80% benign, 20% attack
     is_attack = random.random() < 0.2
    
         if is_attack:
          attack_type, category, severity = random.choice(attack_types)
       prediction = {
           'is_attack': True,
    'attack_type': attack_type,
          'attack_category': category,
                'severity': severity,
        'confidence': random.uniform(0.75, 0.99),
          'model_version': 'v1.0-test'
          }
         else:
    prediction = {
'is_attack': False,
               'confidence': random.uniform(0.85, 0.99),
                 'model_version': 'v1.0-test'
     }
           
   record_id = process_flow_prediction(writer, features, prediction)
           count += 1
    
     if count % 10 == 0:
      print(f"[IDS] Processed {count} flows...")
        
                time.sleep(random.uniform(0.5, 3.0))
 
            except KeyboardInterrupt:
        print(f"\n\nSimulation stopped. Total flows: {count}")
    break
     except Exception as e:
           print(f"Error: {e}")
        time.sleep(1)


if __name__ == "__main__":
    import sys

    if len(sys.argv) > 1 and sys.argv[1] == "--simulate":
        run_simulation()
else:
   print("""
IDS Detection Pipeline - Database Writer (172 Features)
========================================================

This module writes network flow detection results to the IDS database.
Each table contains all 172 network flow features matching your ML pipeline.

Tables:
-------
- RawPackets: All captured flows (before classification)
- Benign_Table: Classified as normal traffic
- Attack_Table: Classified as attacks

Usage:
------
    python detection_pipeline_example.py --simulate

Or use in your pipeline:

from detection_pipeline_example import IDSDatabaseWriter, process_flow_prediction

    conn_str = "Driver={ODBC Driver 17 for SQL Server};Server=localhost;Database=IDS;Trusted_Connection=yes;"
    
    with IDSDatabaseWriter(conn_str) as writer:
        # Your ML model provides features dict with 172 columns
        features = {'SrcIp': '192.168.1.100', 'DstPort': 80, 'Duration': 0.5, ...}
        
      prediction = {'is_attack': True, 'attack_type': 'DoS', 'confidence': 0.95}
   
        record_id = process_flow_prediction(writer, features, prediction)
""")
