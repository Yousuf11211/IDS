namespace IDS.Data.Models
{
    public class LogFile
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string? Exception { get; set; }
        public string? UserId { get; set; }
    }
}
