namespace Ptn.ApiContractChecker.Constants.Conformance;

// islevi: Sema ornegi ve operasyon zinciri adaylarinin butce, skor ve mekanik deger sabitlerini tanimlar.
// sistemdeki gorevi: Ureticilerin rastgele deger veya daginik guven esigi kullanmasini engeller.
public static class SampleGenerationConsts
{
    public const int MaxSamplesPerField = 16;
    public const int DefaultMaxCandidates = 5;
    public const decimal LinkScoreThreshold = 0.65m;
    public const decimal DeclaredLinkScore = 1m;
    public const decimal SchemaMatchScore = 0.8m;
    public const decimal LocationHeaderScore = 0.7m;
    public const decimal NumericBoundaryStep = 1m;
    public const char StringSampleCharacter = 'x';
    public const string InvalidValueSuffix = "__invalid";
    public const string LocationHeaderName = "Location";
    public const string CreatedStatusCode = "201";
    public const string ResponseBodyExpressionPrefix = "$response.body#";
    public const string ResponseHeaderExpressionPrefix = "$response.header.";
    public const string OperationReferencePathsPrefix = "#/paths/";
}
