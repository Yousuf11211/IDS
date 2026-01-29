# =============================================================================
# IDS Detection Pipeline - Database Writer Example
# =============================================================================
#
# This script shows how your detection pipeline can write directly to the 
# Benign_Table and Attack_Table. The IDS web application will automatically
# detect new records and update connected dashboards in real-time.
#
# Requirements:
#   pip install pyodbc
#
# =============================================================================

import pyodbc
from datetime import datetime
from typing import Optional, Dict, Any
import json


class IDSDatabaseWriter:
    """
    Writes detection results to the IDS database.
    The web application monitors these tables and broadcasts updates via SignalR.
    """
  
    def __init__(self, connection_string: str):
        """
        Initialize the database writer.
        
        Args:
          connection_string: ODBC connection string for SQL Server
         
    Example:
            writer = IDSDatabaseWriter(
     "Driver={ODBC Driver 17 for SQL Server};"
  "Server=localhost;"
                "Database=IDS;"
        "Trusted_Connection=yes;"
            )
  """
     self.conn_str = connection_string
  self.conn = None
     self.cursor = None

    def connect(self):
        """Establish database connection."""
        self.conn = pyodbc.connect(self.conn_str)
  self.cursor = self.conn.cursor()
        print("[IDS] Connected to database")
        
    def disconnect(self):
        """Close database connection."""
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
    
    def save_benign(
   self,
        source_ip: str,
        destination_ip: str,
     protocol: str,
        source_port: int = 0,
        destination_port: int = 0,
        packet_size: int = 0,
        duration: float = 0.0,
        bytes_sent: int = 0,
        bytes_received: int = 0,
        service: Optional[str] = None,
        confidence_score: float = 0.95,
    feature_vector: Optional[Dict] = None,
        model_version: str = "v1.0"
    ) -> int:
        """
        Save benign traffic detection to the database.
   
        Returns:
      The ID of the inserted record.
  """
  feature_json = json.dumps(feature_vector) if feature_vector else None
        
     self.cursor.execute("""
            INSERT INTO Benign_Table 
          (DetectedAt, SourceIP, DestinationIP, SourcePort, DestinationPort,
          Protocol, PacketSize, Duration, BytesSent, BytesReceived,
         Service, ConfidenceScore, FeatureVector, ModelVersion)
     OUTPUT INSERTED.Id
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
 """, 
  datetime.utcnow(),
            source_ip,
            destination_ip,
            source_port,
       destination_port,
       protocol,
        packet_size,
       duration,
            bytes_sent,
            bytes_received,
            service,
  confidence_score,
 feature_json,
            model_version
)
    
        row = self.cursor.fetchone()
        self.conn.commit()
        
        record_id = row[0] if row else 0
        return record_id
    
 def save_attack(
    self,
        source_ip: str,
        destination_ip: str,
        protocol: str,
        attack_type: str,
 severity: str = "Medium",
        source_port: int = 0,
        destination_port: int = 0,
        packet_size: int = 0,
        duration: float = 0.0,
      bytes_sent: int = 0,
        bytes_received: int = 0,
        service: Optional[str] = None,
    attack_category: Optional[str] = None,
        confidence_score: float = 0.95,
        feature_vector: Optional[Dict] = None,
    model_version: str = "v1.0"
    ) -> int:
   """
        Save attack detection to the database.
     
        Args:
        attack_type: Type of attack (DoS, DDoS, PortScan, BruteForce, etc.)
        severity: Severity level (Critical, High, Medium, Low)
         attack_category: Optional category (DoS, Probe, R2L, U2R)
        
        Returns:
            The ID of the inserted record.
      """
     feature_json = json.dumps(feature_vector) if feature_vector else None
        
        self.cursor.execute("""
       INSERT INTO Attack_Table 
            (DetectedAt, SourceIP, DestinationIP, SourcePort, DestinationPort,
        Protocol, PacketSize, Duration, BytesSent, BytesReceived,
  Service, AttackType, AttackCategory, Severity, 
    ConfidenceScore, FeatureVector, ModelVersion)
    OUTPUT INSERTED.Id
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    """,
            datetime.utcnow(),
        source_ip,
     destination_ip,
   source_port,
  destination_port,
      protocol,
         packet_size,
            duration,
      bytes_sent,
    bytes_received,
   service,
    attack_type,
 attack_category,
  severity,
        confidence_score,
         feature_json,
         model_version
   )
        
  row = self.cursor.fetchone()
        self.conn.commit()

        record_id = row[0] if row else 0
     print(f"[IDS] Attack saved: {attack_type} ({severity}) from {source_ip} - ID: {record_id}")
 return record_id


def determine_severity(attack_type: str, confidence: float) -> str:
    """
    Determine severity based on attack type and confidence score.
 Customize this based on your needs.
    """
    attack_type_lower = attack_type.lower()
    
    # Critical attacks
    if attack_type_lower in ["dos", "ddos", "infiltration", "botnet", "ransomware"]:
        return "Critical" if confidence > 0.9 else "High"
    
    # High severity
    if attack_type_lower in ["portscan", "bruteforce", "sql_injection", "xss"]:
 return "High" if confidence > 0.8 else "Medium"
    
    # Medium severity by default
    if confidence > 0.7:
        return "Medium"

    return "Low"


# =============================================================================
# Example: Integration with your ML Model
# =============================================================================

def process_detection(writer: IDSDatabaseWriter, packet_features: Dict, model_output: Dict):
    """
  Process a detection from your ML model and save to database.
    
    Args:
      writer: IDSDatabaseWriter instance
   packet_features: Dictionary of extracted network features
      model_output: Dictionary with model prediction results
    """
    
    # Extract common fields
    src_ip = packet_features.get('src_ip', '0.0.0.0')
    dst_ip = packet_features.get('dst_ip', '0.0.0.0')
    protocol = packet_features.get('protocol', 'TCP')
    
    if model_output.get('is_attack', False):
    # It's an attack
        attack_type = model_output.get('attack_type', 'Unknown')
        confidence = model_output.get('confidence', 0.5)
   severity = model_output.get('severity') or determine_severity(attack_type, confidence)
        
        writer.save_attack(
         source_ip=src_ip,
            destination_ip=dst_ip,
      protocol=protocol,
         attack_type=attack_type,
      severity=severity,
      source_port=packet_features.get('src_port', 0),
            destination_port=packet_features.get('dst_port', 0),
packet_size=packet_features.get('packet_size', 0),
            duration=packet_features.get('duration', 0.0),
    bytes_sent=packet_features.get('bytes_sent', 0),
     bytes_received=packet_features.get('bytes_received', 0),
      service=packet_features.get('service'),
            attack_category=model_output.get('attack_category'),
            confidence_score=confidence,
            feature_vector=packet_features,
            model_version=model_output.get('model_version', 'v1.0')
     )
    else:
        # It's benign traffic
        writer.save_benign(
        source_ip=src_ip,
 destination_ip=dst_ip,
       protocol=protocol,
source_port=packet_features.get('src_port', 0),
     destination_port=packet_features.get('dst_port', 0),
    packet_size=packet_features.get('packet_size', 0),
   duration=packet_features.get('duration', 0.0),
bytes_sent=packet_features.get('bytes_sent', 0),
       bytes_received=packet_features.get('bytes_received', 0),
            service=packet_features.get('service'),
            confidence_score=model_output.get('confidence', 0.95),
            feature_vector=packet_features,
       model_version=model_output.get('model_version', 'v1.0')
        )


# =============================================================================
# Example: Simulated Detection Pipeline (for testing)
# =============================================================================

def run_simulation():
    """
    Run a simulation that generates random detections.
    Use this to test the live dashboard updates.
    """
import random
    import time
    
    # Connection string - modify for your setup
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
    
    protocols = ["TCP", "UDP", "HTTP", "HTTPS", "SSH", "FTP"]
    services = ["http", "https", "ssh", "ftp", "smtp", "dns", None]
    
    print("Starting detection simulation...")
    print("Press Ctrl+C to stop\n")
    
    with IDSDatabaseWriter(conn_str) as writer:
        count = 0
while True:
 try:
        # Generate random IPs
     src_ip = f"192.168.{random.randint(1, 255)}.{random.randint(1, 255)}"
              dst_ip = f"10.0.{random.randint(1, 10)}.{random.randint(1, 255)}"
    
      # Random packet features
        features = {
 'src_ip': src_ip,
    'dst_ip': dst_ip,
   'src_port': random.randint(1024, 65535),
          'dst_port': random.choice([80, 443, 22, 21, 25, 53, 3306]),
           'protocol': random.choice(protocols),
     'packet_size': random.randint(64, 1500),
     'duration': random.uniform(0.001, 5.0),
    'bytes_sent': random.randint(100, 50000),
   'bytes_received': random.randint(100, 50000),
 'service': random.choice(services),
 }
            
           # 80% benign, 20% attack
       is_attack = random.random() < 0.2
          
       if is_attack:
 attack_type, category, severity = random.choice(attack_types)
          model_output = {
'is_attack': True,
             'attack_type': attack_type,
   'attack_category': category,
      'severity': severity,
            'confidence': random.uniform(0.75, 0.99),
             'model_version': 'v1.0-test'
      }
       else:
           model_output = {
    'is_attack': False,
'confidence': random.uniform(0.85, 0.99),
         'model_version': 'v1.0-test'
     }
   
       process_detection(writer, features, model_output)
        count += 1
           
            if count % 10 == 0:
     print(f"[IDS] Processed {count} detections...")
             
              # Random delay between 0.5 and 3 seconds
      time.sleep(random.uniform(0.5, 3.0))
 
         except KeyboardInterrupt:
    print(f"\n\nSimulation stopped. Total detections: {count}")
  break
     except Exception as e:
    print(f"Error: {e}")
     time.sleep(1)


# =============================================================================
# Main Entry Point
# =============================================================================

if __name__ == "__main__":
 import sys
    
    if len(sys.argv) > 1 and sys.argv[1] == "--simulate":
        run_simulation()
    else:
        print("""
IDS Detection Pipeline - Database Writer
=========================================

This module writes detection results directly to the IDS database.
The IDS web application monitors these tables and broadcasts updates
to connected dashboards in real-time via SignalR.

Usage:
------
    python detection_pipeline_example.py --simulate

Or import and use in your detection pipeline:

    from detection_pipeline_example import IDSDatabaseWriter

    conn_str = (
        "Driver={ODBC Driver 17 for SQL Server};"
"Server=localhost;"
      "Database=IDS;"
        "Trusted_Connection=yes;"
    )
    
    with IDSDatabaseWriter(conn_str) as writer:
        # Save benign traffic
        writer.save_benign(
     source_ip="192.168.1.100",
    destination_ip="10.0.0.5",
            protocol="TCP",
            confidence_score=0.98
        )
 
        # Save attack
        writer.save_attack(
source_ip="192.168.1.200",
            destination_ip="10.0.0.5",
       protocol="TCP",
        attack_type="DoS",
 severity="Critical",
            confidence_score=0.95
        )

Database Tables:
----------------
- Benign_Table: Stores normal/safe traffic
- Attack_Table: Stores detected attacks

The web app polls these tables every 2 seconds and broadcasts
new records to all connected dashboard users.
""")
