# =============================================================================
# IDS Detection Pipeline Integration Example
# =============================================================================
# This script shows how your detection pipeline can send data to the IDS web app.
# The web app will receive the data and broadcast it to all connected dashboards
# in real-time via SignalR.
#
# Requirements:
#   pip install requests
#
# =============================================================================

import requests
import json
import time
import random
from datetime import datetime

# =============================================================================
# Configuration
# =============================================================================
IDS_API_BASE_URL = "https://localhost:7001"  # Change to your IDS web app URL
API_KEY = "your-api-key-here"  # Set this in appsettings.json or environment

# Headers for API requests
HEADERS = {
    "Content-Type": "application/json",
    "X-Api-Key": API_KEY
}

# =============================================================================
# API Functions
# =============================================================================

def send_benign_traffic(data: dict) -> dict:
    """
 Send a single benign traffic detection to the IDS.
    
    Args:
        data: Dictionary with traffic information
        
    Returns:
   API response as dictionary
    """
    url = f"{IDS_API_BASE_URL}/api/detection/benign"
    response = requests.post(url, json=data, headers=HEADERS, verify=False)
    return response.json()


def send_attack(data: dict) -> dict:
    """
    Send a single attack detection to the IDS.
    
    Args:
        data: Dictionary with attack information
        
    Returns:
        API response as dictionary
    """
    url = f"{IDS_API_BASE_URL}/api/detection/attack"
    response = requests.post(url, json=data, headers=HEADERS, verify=False)
    return response.json()


def send_batch(benign_list: list, attack_list: list) -> dict:
    """
    Send multiple detections in a single batch request (more efficient).
    
    Args:
        benign_list: List of benign traffic dictionaries
        attack_list: List of attack dictionaries
        
    Returns:
        API response as dictionary
    """
  url = f"{IDS_API_BASE_URL}/api/detection/batch"
    data = {
      "benignTraffic": benign_list,
        "attacks": attack_list
    }
    response = requests.post(url, json=data, headers=HEADERS, verify=False)
    return response.json()


def health_check() -> bool:
    """Check if the IDS API is healthy."""
    try:
      url = f"{IDS_API_BASE_URL}/api/detection/health"
     response = requests.get(url, verify=False, timeout=5)
        return response.status_code == 200
    except:
        return False


# =============================================================================
# Example: Integration with your ML Model
# =============================================================================

def process_network_packet(packet_features: dict, model_prediction: dict):
    """
    Example function showing how to integrate with your ML model.
    
    Args:
     packet_features: Dictionary of extracted network features
    model_prediction: Dictionary with 'is_attack', 'attack_type', 'confidence'
    """
    
    # Common fields for both benign and attack
    base_data = {
  "sourceIP": packet_features.get("src_ip", "0.0.0.0"),
      "destinationIP": packet_features.get("dst_ip", "0.0.0.0"),
        "sourcePort": packet_features.get("src_port", 0),
      "destinationPort": packet_features.get("dst_port", 0),
        "protocol": packet_features.get("protocol", "TCP"),
        "packetSize": packet_features.get("packet_size", 0),
        "duration": packet_features.get("duration", 0.0),
     "bytesSent": packet_features.get("bytes_sent", 0),
     "bytesReceived": packet_features.get("bytes_received", 0),
   "service": packet_features.get("service"),
        "confidenceScore": model_prediction.get("confidence", 0.95),
        "featureVector": json.dumps(packet_features),  # Optional: store raw features
   "modelVersion": "v1.0.0"  # Track which model version made the prediction
    }
    
    if model_prediction.get("is_attack", False):
     # It's an attack - add attack-specific fields
  attack_data = {
       **base_data,
  "attackType": model_prediction.get("attack_type", "Unknown"),
    "attackCategory": model_prediction.get("attack_category"),
            "severity": determine_severity(model_prediction)
        }
   
        result = send_attack(attack_data)
        print(f"[ATTACK] {attack_data['attackType']} from {attack_data['sourceIP']} - {result}")
        
    else:
    # It's benign traffic
 result = send_benign_traffic(base_data)
     print(f"[BENIGN] {base_data['sourceIP']} -> {base_data['destinationIP']} - {result}")


def determine_severity(prediction: dict) -> str:
    """Determine severity based on attack type and confidence."""
    attack_type = prediction.get("attack_type", "").lower()
    confidence = prediction.get("confidence", 0.5)
    
    # Critical attacks
    if attack_type in ["dos", "ddos", "infiltration", "botnet"]:
        return "Critical" if confidence > 0.9 else "High"
    
    # High severity attacks
    if attack_type in ["portscan", "bruteforce", "sql_injection"]:
        return "High" if confidence > 0.8 else "Medium"
    
    # Medium severity
    if confidence > 0.7:
        return "Medium"
    
    return "Low"


# =============================================================================
# Example: Simulated Detection Pipeline
# =============================================================================

def simulate_detection_pipeline():
    """
    Simulates a detection pipeline for testing.
 Generates random benign and attack traffic.
    """
    
    print("Starting simulated detection pipeline...")
    print(f"API URL: {IDS_API_BASE_URL}")
    
    # Check API health
    if not health_check():
     print("ERROR: IDS API is not reachable!")
        return
    
    print("API is healthy. Starting simulation...")
    
    # Attack types for simulation
    attack_types = [
        ("DoS", "DoS", "Critical"),
        ("DDoS", "DoS", "Critical"),
        ("PortScan", "Probe", "Medium"),
        ("BruteForce", "Probe", "High"),
        ("SQLInjection", "R2L", "High"),
        ("Infiltration", "U2R", "Critical"),
        ("Botnet", "Malware", "Critical"),
    ]
    
    protocols = ["TCP", "UDP", "ICMP", "HTTP", "HTTPS", "SSH", "FTP"]
    services = ["http", "https", "ssh", "ftp", "smtp", "dns", "mysql", None]
    
    while True:
        try:
     # Generate random source and destination IPs
    src_ip = f"192.168.{random.randint(1, 255)}.{random.randint(1, 255)}"
            dst_ip = f"10.0.{random.randint(1, 10)}.{random.randint(1, 255)}"
     
 # Random packet features
      features = {
 "src_ip": src_ip,
     "dst_ip": dst_ip,
    "src_port": random.randint(1024, 65535),
       "dst_port": random.choice([80, 443, 22, 21, 25, 53, 3306, 8080]),
    "protocol": random.choice(protocols),
        "packet_size": random.randint(64, 1500),
           "duration": random.uniform(0.001, 10.0),
   "bytes_sent": random.randint(100, 100000),
    "bytes_received": random.randint(100, 100000),
            "service": random.choice(services),
 }
       
         # 80% benign, 20% attack
      is_attack = random.random() < 0.2
            
          if is_attack:
    attack_type, category, severity = random.choice(attack_types)
           prediction = {
      "is_attack": True,
  "attack_type": attack_type,
      "attack_category": category,
          "confidence": random.uniform(0.75, 0.99)
 }
            else:
 prediction = {
           "is_attack": False,
       "confidence": random.uniform(0.85, 0.99)
       }
        
          # Process the packet
       process_network_packet(features, prediction)
        
            # Random delay between 0.1 and 2 seconds
 time.sleep(random.uniform(0.1, 2.0))
            
   except KeyboardInterrupt:
        print("\nSimulation stopped.")
            break
        except Exception as e:
            print(f"Error: {e}")
      time.sleep(1)


# =============================================================================
# Example: Batch Processing
# =============================================================================

def batch_process_detections(detections: list):
"""
    Process a batch of detections efficiently.
    
    Args:
    detections: List of (features, prediction) tuples
    """
    benign_list = []
    attack_list = []
    
    for features, prediction in detections:
        base_data = {
    "sourceIP": features.get("src_ip"),
     "destinationIP": features.get("dst_ip"),
            "sourcePort": features.get("src_port", 0),
            "destinationPort": features.get("dst_port", 0),
       "protocol": features.get("protocol", "TCP"),
    "packetSize": features.get("packet_size", 0),
   "duration": features.get("duration", 0.0),
         "bytesSent": features.get("bytes_sent", 0),
     "bytesReceived": features.get("bytes_received", 0),
            "service": features.get("service"),
            "confidenceScore": prediction.get("confidence", 0.95),
            "modelVersion": "v1.0.0"
        }
        
     if prediction.get("is_attack"):
       attack_list.append({
      **base_data,
"attackType": prediction.get("attack_type", "Unknown"),
     "attackCategory": prediction.get("attack_category"),
   "severity": determine_severity(prediction)
     })
        else:
 benign_list.append(base_data)
    
    # Send batch
    result = send_batch(benign_list, attack_list)
    print(f"Batch processed: {result}")


# =============================================================================
# Main Entry Point
# =============================================================================

if __name__ == "__main__":
    import sys
    import urllib3
  
    # Disable SSL warnings for development
    urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
    
  if len(sys.argv) > 1 and sys.argv[1] == "--simulate":
    simulate_detection_pipeline()
    else:
        print("""
IDS Detection Pipeline Integration
===================================

Usage:
    python detection_pipeline_example.py --simulate  # Run simulation
    
Or import and use in your code:

    from detection_pipeline_example import send_attack, send_benign_traffic
    
    # Send an attack detection
    send_attack({
        "sourceIP": "192.168.1.100",
        "destinationIP": "10.0.0.5",
        "protocol": "TCP",
        "attackType": "DoS",
 "severity": "Critical",
        "confidenceScore": 0.95
    })
    
    # Send benign traffic
    send_benign_traffic({
        "sourceIP": "192.168.1.50",
     "destinationIP": "10.0.0.5",
   "protocol": "HTTP",
        "confidenceScore": 0.98
    })
""")
