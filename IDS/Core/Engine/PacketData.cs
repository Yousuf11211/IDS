namespace IDS.Core.Models
{
    // Rename this based on your CSV column names and types
    public class PacketData
    {
        public float Feature1 { get; set; } // Example network feature
        public float Feature2 { get; set; }
        // ... include all features needed by your ML models ...

        // Output from Anomaly Detection Model
        public bool IsAnomaly { get; set; }
        public float AnomalyScore { get; set; }

        // Output from Multi-Class Detection Model
        public string AttackType { get; set; } = "Benign"; // e.g., DDoS, XSS, SQLInjection
    }
}