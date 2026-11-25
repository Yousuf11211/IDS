namespace IDS.Core.Services;

public sealed record MonitoringWindow(DateTime From, DateTime To, bool Compare)
{
    public static readonly string[] LiveRanges = ["5m", "15m", "30m", "1h", "24h"];
    public static readonly string[] DashboardRanges = ["1h", "24h", "7d", "30d", "all", "custom"];

    public static MonitoringWindow? Resolve(string range, DateTime now, DateOnly? from = null, DateOnly? through = null)
    {
        var minutes = range switch
        {
            "5m" => 5, "15m" => 15, "30m" => 30, "1h" => 60, "24h" => 1440,
            "7d" => 10080, "30d" => 43200, _ => 0
        };
        if (minutes > 0) return new(now.AddMinutes(-minutes), now, true);
        if (range == "all") return new(DateTime.MinValue, now, false);
        if (range != "custom" || from is null || through is null || through < from ||
            from.Value.Year < 1900 || through.Value.Year >= 9999) return null;

        var start = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = through.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        if (start >= now) return null;
        return new(start, end <= now ? end : now, end <= now);
    }

    public (DateTime Start, int Minutes, int Count) Buckets(DateTime first)
    {
        var start = From == DateTime.MinValue ? first : From;
        var duration = (To - start).TotalMinutes;
        int[] increments = [1, 2, 5, 10, 15, 30, 60, 120, 360, 720, 1440, 10080, 43200];
        var minutes = increments.FirstOrDefault(value => duration / value <= 72);
        if (minutes == 0) minutes = (int)Math.Ceiling(duration / 72);
        return (start, minutes, Math.Max(1, (int)Math.Ceiling(duration / minutes)));
    }
}
