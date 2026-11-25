namespace IDS.Core.Services;

public static class DetectionLabels
{
    public const string Unclassified = "Unclassified";

    // The model pipeline stores UnknownAttack when the selected class falls
    // below threshold. Attack alone is the legacy binary result.
    public static readonly string[] UnclassifiedValues = ["UnknownAttack", "Attack", Unclassified];

    public static string Display(string? value) => string.IsNullOrWhiteSpace(value) ||
        UnclassifiedValues.Contains(value, StringComparer.OrdinalIgnoreCase) ? Unclassified : value;
}
