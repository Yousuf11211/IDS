using IDS.Data;
using Microsoft.EntityFrameworkCore;

namespace IDS.Core.Services;

public static class OperationsSettings
{
    public const string EscalationKey = "Operations.IncidentEscalationEnabled";
    public const string AssignmentKey = "Operations.IncidentAssignmentEnabled";
    public const string AlertLimitKey = "Operations.RecentAlertLimit";
    public const string ShapJobsKey = "Operations.ShapJobGenerationEnabled";
    public const string RefreshKey = "Operations.AnalyticsRefreshSeconds";

    public const string DashboardAutoRefreshKey = "Operations.DashboardAutoRefresh";
    public const string LiveRefreshKey = "Operations.LiveRefreshSeconds";
    public const string DashboardRangeKey = "Operations.DashboardRange";
    public const string LiveRangeKey = "Operations.LiveRange";
    public const string TrafficSeriesKey = "Operations.DefaultTrafficSeries";
    public const string DashboardAlertLimitKey = "Operations.DashboardAlertLimit";
    public const string RecentDetectionsKey = "Operations.RecentDetectionLimit";

    public static async Task<Dictionary<string, string>> ReadAsync(ApplicationDbContext db, string prefix,
        CancellationToken token = default) => await db.SystemSettings.AsNoTracking()
        .Where(s => s.Key.StartsWith(prefix)).ToDictionaryAsync(s => s.Key, s => s.Value, token);

    public static bool Enabled(this IReadOnlyDictionary<string, string> settings, string key, bool fallback = true)
        => bool.TryParse(settings.GetValueOrDefault(key), out var enabled) ? enabled : fallback;

    public static int Number(this IReadOnlyDictionary<string, string> settings, string key, int fallback, int min, int max)
        => Math.Clamp(int.TryParse(settings.GetValueOrDefault(key), out var value) ? value : fallback, min, max);

    public static Task<bool> IsEscalationEnabledAsync(ApplicationDbContext db) => EnabledAsync(db, EscalationKey);
    public static Task<bool> IsAssignmentEnabledAsync(ApplicationDbContext db) => EnabledAsync(db, AssignmentKey);

    public static Task<bool> GeneratesShapJobsAsync(ApplicationDbContext db) => EnabledAsync(db, ShapJobsKey);

    public static async Task<int> NumberAsync(ApplicationDbContext db, string key, int fallback,
        int minimum, int maximum, CancellationToken token)
    {
        var value = await db.SystemSettings.AsNoTracking().Where(s => s.Key == key)
            .Select(s => s.Value).SingleOrDefaultAsync(token);
        return Math.Clamp(int.TryParse(value, out var number) ? number : fallback, minimum, maximum);
    }

    private static async Task<bool> EnabledAsync(ApplicationDbContext db, string key)
    {
        var value = await db.SystemSettings.AsNoTracking().Where(s => s.Key == key)
            .Select(s => s.Value).SingleOrDefaultAsync();
        return value == null || (bool.TryParse(value, out var enabled) && enabled);
    }
}
