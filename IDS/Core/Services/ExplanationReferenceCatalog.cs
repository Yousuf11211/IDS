using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IDS.Data.Models;

namespace IDS.Core.Services;

/// <summary>Loads deployment-owned explainability assets once. Stored SHAP data is read-only.</summary>
public sealed class ExplanationReferenceCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Lazy<ProfileResult> profile;
    private readonly Lazy<IReadOnlyDictionary<string, InspectionFeatureMetadata>> featureMetadata;

    private sealed record Profile(BenignReferenceMetadata Metadata, string[] Names,
        Dictionary<string, BenignFeatureStatistics> Features);
    private sealed record ProfileResult(Profile? Profile, string? Diagnostic);

    public ExplanationReferenceCatalog(IConfiguration config, IWebHostEnvironment environment,
        ILogger<ExplanationReferenceCatalog> logger)
    {
        var assets = Path.Combine(AppContext.BaseDirectory, "Explainability");
        var configured = Environment.GetEnvironmentVariable("Explainability__BenignReferencePath")
            ?? config["Explainability:BenignReferencePath"];
        var profilePath = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(assets, "benign-reference.json") : Path.Combine(environment.ContentRootPath, configured);
        profile = new(() => LoadProfile(profilePath, logger));
        featureMetadata = new(() => LoadFeatures(Path.Combine(assets, "feature-metadata.json"), logger));
    }

    public void Enrich(ModelExplanationData data)
    {
        if (data.Features.Count == 0) return;
        var reference = profile.Value;
        var snapshotHasReference = data.Reference != null && data.Features.Any(feature => feature.Reference != null);
        Profile? selected = null;
        if (!snapshotHasReference)
        {
            if (reference.Profile is not { } candidate)
                data.ReferenceDiagnostic = reference.Diagnostic;
            else if (!string.Equals(candidate.Metadata.ModelFingerprint, data.Fingerprint, StringComparison.Ordinal) ||
                data.FeatureNames == null || !data.FeatureNames.SequenceEqual(candidate.Names, StringComparer.Ordinal) ||
                !data.Features.Select(feature => feature.Name).SequenceEqual(candidate.Names, StringComparer.Ordinal))
                data.ReferenceDiagnostic = "The installed benign reference is for a different model release or feature order.";
            else if (data.Reference != null &&
                !string.Equals(data.Reference.DatasetSha256, candidate.Metadata.DatasetSha256, StringComparison.Ordinal))
                data.ReferenceDiagnostic = "The training reference recorded for this explanation is unavailable.";
            else
            {
                data.Reference = candidate.Metadata;
                selected = candidate;
            }
            if (selected == null) data.Reference = null;
        }

        data.Features = data.Features.Select(feature =>
        {
            var metadata = featureMetadata.Value.GetValueOrDefault(feature.Name);
            var stats = snapshotHasReference ? feature.Reference : selected?.Features.GetValueOrDefault(feature.Name);
            return feature with { Units = metadata?.Units, Metadata = metadata, Reference = ForDisplay(stats, feature.Value) };
        }).ToList();
    }

    private static BenignFeatureStatistics? ForDisplay(BenignFeatureStatistics? stats, double? observed)
    {
        if (stats?.Frequencies is not { Count: > 0 } frequencies) return stats;
        var match = frequencies.FirstOrDefault(item => item.Value == observed);
        long? observedCount = observed == null ? null : match?.Count ?? (stats.OtherCount == 0 ? 0L : null);
        var shown = frequencies.Take(8).ToList();
        if (match != null && !shown.Contains(match)) shown.Add(match);
        return stats with
        {
            Frequencies = shown, ObservedCount = observedCount, OtherCount = stats.SampleCount - shown.Sum(item => item.Count)
        };
    }

    private static ProfileResult LoadProfile(string path, ILogger logger)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var file = JsonDocument.Parse(stream);
            var root = file.RootElement;
            var metadata = root.Deserialize<BenignReferenceMetadata>(JsonOptions) ?? throw new JsonException("Missing reference metadata");
            var names = root.GetProperty("featureNames").Deserialize<string[]>(JsonOptions) ?? [];
            var features = root.GetProperty("features").Deserialize<Dictionary<string, BenignFeatureStatistics>>(JsonOptions) ?? [];
            var schemaHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', names))));
            if (metadata.SchemaVersion != 2 || metadata.Split != "training" ||
                metadata.BenignSampleCount is not > 0 || root.GetProperty("featureSchemaVersion").GetInt32() <= 0 ||
                !ValidDigest(metadata.ModelFingerprint) ||
                !ValidDigest(metadata.DatasetSha256) || string.IsNullOrWhiteSpace(metadata.Provenance) ||
                string.IsNullOrWhiteSpace(metadata.DatasetVersion) || string.IsNullOrWhiteSpace(metadata.PreprocessingVersion) ||
                names.Length != 120 || names.Distinct(StringComparer.Ordinal).Count() != names.Length ||
                features.Count != names.Length || names.Any(name => string.IsNullOrWhiteSpace(name) || !features.ContainsKey(name)) ||
                !string.Equals(root.GetProperty("featureSchemaSha256").GetString(), schemaHash, StringComparison.OrdinalIgnoreCase) ||
                features.Values.Any(stats => !ValidStats(stats, metadata.BenignSampleCount.Value)))
                throw new JsonException("The training profile is incomplete or invalid.");
            return new(new(metadata, names, features), null);
        }
        catch (FileNotFoundException)
        {
            logger.LogWarning("Benign reference artifact is missing. Configure Explainability:BenignReferencePath or deploy the bundled profile.");
            return new(null, "The benign reference artifact is missing. Deploy the verified training profile to enable comparison.");
        }
        catch (DirectoryNotFoundException)
        {
            logger.LogWarning("The benign reference asset directory is missing.");
            return new(null, "The benign reference artifact is missing. Deploy the verified training profile to enable comparison.");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or
            KeyNotFoundException or InvalidOperationException or ArgumentException or OverflowException or FormatException)
        {
            logger.LogWarning("The benign reference artifact could not be loaded or validated ({ErrorType}).", error.GetType().Name);
            return new(null, "The benign reference artifact could not be loaded or validated. Ask an administrator to check the server configuration.");
        }
    }

    private static bool ValidDigest(string? value) => value is { Length: 64 } && value.All(Uri.IsHexDigit);

    private static bool ValidStats(BenignFeatureStatistics? stats, long samples)
    {
        if (stats == null || stats.SampleCount < 0 || stats.MissingCount is not >= 0 ||
            stats.SampleCount + stats.MissingCount != samples ||
            stats.Kind is not ("continuous" or "discrete" or "categorical" or "binary")) return false;
        if (stats.SampleCount == 0) return true;
        double?[] bounds = [stats.Minimum, stats.P05, stats.P25, stats.Median, stats.P75, stats.P95, stats.Maximum];
        if (bounds.Any(value => value is not double number || !double.IsFinite(number)) || !bounds.SequenceEqual(bounds.Order()) ||
            stats.Mean is not double mean || !double.IsFinite(mean) ||
            stats.StandardDeviation is not double deviation || !double.IsFinite(deviation) || deviation < 0) return false;
        if (stats.Kind == "continuous") return true;
        return stats.OtherCount is >= 0 && stats.Frequencies is { Count: > 0 } frequencies &&
            frequencies.Select(item => item.Value).Distinct().Count() == frequencies.Count &&
            frequencies.All(item => double.IsFinite(item.Value) && item.Count > 0 && item.Count <= samples) &&
            frequencies.Sum(item => item.Count) + stats.OtherCount == stats.SampleCount;
    }

    private static IReadOnlyDictionary<string, InspectionFeatureMetadata> LoadFeatures(string path, ILogger logger)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var file = JsonDocument.Parse(stream);
            var features = file.RootElement.GetProperty("features").Deserialize<Dictionary<string, InspectionFeatureMetadata>>(JsonOptions);
            if (file.RootElement.GetProperty("schemaVersion").GetInt32() != 1 || features is not { Count: 120 } ||
                features.Values.Any(item => item == null || string.IsNullOrWhiteSpace(item.DisplayName) ||
                    string.IsNullOrWhiteSpace(item.Description) || string.IsNullOrWhiteSpace(item.Category)))
                throw new JsonException("Invalid feature metadata");
            return features;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or
            KeyNotFoundException or InvalidOperationException)
        {
            logger.LogWarning("Inspection feature metadata could not be loaded ({ErrorType}).", error.GetType().Name);
            return new Dictionary<string, InspectionFeatureMetadata>();
        }
    }
}
