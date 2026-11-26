namespace IDS.Data.Models;

public sealed record InspectionReport(string Id, string AttackType, string Severity, DateTime Timestamp,
    string? SourceIp, int? DestinationPort, bool IsAcknowledged, bool IsEscalated,
    double CategoryProbability, string? ModelRelease, IReadOnlyList<InspectionStage> Stages,
    IReadOnlyDictionary<string, object?> StoredFlow)
{
    public DateTimeOffset? CsvReceivedAt { get; init; }
    public DateTimeOffset? ClassificationRecordedAt { get; init; }
}

public sealed record InspectionStage(string Id, string Stage, string Status, string ModelVersion,
    int Attempts, string? Error, DateTime UpdatedAt, ModelExplanationData? Data);

public sealed class ModelExplanationData
{
    public int SchemaVersion { get; set; }
    public string? AttackId { get; set; }
    public string? RawId { get; set; }
    public string? Release { get; set; }
    public string? Fingerprint { get; set; }
    public string? ModelName { get; set; }
    public string? ModelVersion { get; set; }
    public int? ClassIndex { get; set; }
    public string? ExplainedClass { get; set; }
    public string? PredictedLabel { get; set; }
    public double? Probability { get; set; }
    public double? Threshold { get; set; }
    public List<ClassProbability> TopProbabilities { get; set; } = new();
    public string? OutputUnits { get; set; }
    public double? BaseValue { get; set; }
    public double? RawMargin { get; set; }
    public double? ReconstructedMargin { get; set; }
    public double? AdditivityError { get; set; }
    public double? GenerationMs { get; set; }
    public double? ProcessingMs { get; set; }
    public double? QueueWaitMs { get; set; }
    public InferenceBatchTiming? PipelineTiming { get; set; }
    public DateTimeOffset? GeneratedAt { get; set; }
    public string? ShapVersion { get; set; }
    public string? XgboostVersion { get; set; }
    public string? AdapterNote { get; set; }
    public List<string> FeatureNames { get; set; } = new();
    public List<double?> FeatureValues { get; set; } = new();
    public List<ExplainedFeature> Features { get; set; } = new();
    public BenignReferenceMetadata? Reference { get; set; }
    public string? ReferenceDiagnostic { get; set; }
}

public sealed record ClassProbability(string Label, double Probability);
public sealed record ExplainedFeature(string Name, double? Value, double Contribution, string? Units,
    BenignFeatureStatistics? Reference)
{
    public InspectionFeatureMetadata? Metadata { get; init; }
}

public sealed record InspectionFeatureMetadata(string DisplayName, string Description, string? Units,
    string Category, string Kind, string Source)
{
    public double? ExpectedMinimum { get; init; }
    public double? ExpectedMaximum { get; init; }
}

public sealed record ReferenceFrequency(double Value, long Count);
public sealed record BenignFeatureStatistics(double? Median, double? Mean, double? StandardDeviation,
    double? P05, double? P25, double? P75, double? P95, long SampleCount)
{
    public double? Minimum { get; init; }
    public double? Maximum { get; init; }
    public string? Kind { get; init; }
    public long? MissingCount { get; init; }
    public IReadOnlyList<ReferenceFrequency>? Frequencies { get; init; }
    public long? OtherCount { get; init; }
    public long? ObservedCount { get; init; }
    public long? DistinctValues { get; init; }
}

public sealed record BenignReferenceMetadata(string DatasetVersion, string DatasetSha256,
    string Split, string Provenance, string Group)
{
    public int? SchemaVersion { get; init; }
    public string? ModelFingerprint { get; init; }
    public string? ModelRelease { get; init; }
    public string? PreprocessingVersion { get; init; }
    public long? BenignSampleCount { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}

public sealed record InferenceBatchTiming(double? BinaryMs, int? BinaryBatchSize, double? MulticlassMs,
    int? MulticlassBatchSize, double? InferenceMs);
