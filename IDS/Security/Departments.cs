namespace IDS.Security;

/// <summary>
/// Shared department names for administration and colleague discovery.
/// Keeping one list prevents spelling differences from splitting a department.
/// </summary>
public static class Departments
{
    public const string General = "General";

    public static readonly IReadOnlyList<string> All =
    [
        General,
        "Security Operations",
        "IT Support",
        "Engineering",
        "Management"
    ];

    public static bool IsKnown(string? department)
        => department != null && All.Contains(department, StringComparer.Ordinal);
}
