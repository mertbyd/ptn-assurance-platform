using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ptn.DatabaseChecker.Dtos.Correlation;

namespace Ptn.DatabaseChecker.Dtos.Diagnosis;

// islevi: RFC 9457 alanlari ile checknexus kimlik, konum, hipotez ve next-check uzantilarini API cevabinda tasir.
// sistemdeki gorevi: Test Module ve MCP'ye 4 KB altinda sirali, kanitli ve kararli teshis sozlesmesi verir.
public sealed class DiagnosisReportDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;

    [JsonPropertyName("instance")]
    public string Instance { get; set; } = string.Empty;

    [JsonPropertyName("checknexus:identity")]
    public IdentityDto Identity { get; set; } = new();

    [JsonPropertyName("checknexus:location")]
    public LocationDto Location { get; set; } = new();

    [JsonPropertyName("checknexus:hypotheses")]
    public List<HypothesisDto> Hypotheses { get; set; } = new();

    [JsonPropertyName("checknexus:nextChecks")]
    public List<string> NextChecks { get; set; } = new();

    [JsonPropertyName("checknexus:correlation")]
    public CorrelationRefDto? Correlation { get; set; }

    // islevi: Cikarilan hata kodu, sinifi, kaynak, engine, guven, olgular ve dogrulanmis referanslari tasir.
    // sistemdeki gorevi: Modelin provider metni okumadan hangi kimlik uzerinden karar verdigini aciklar.
    public sealed class IdentityDto
    {
        public string SourceKindCode { get; set; } = string.Empty;
        public string EngineCode { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string CodeClassCode { get; set; } = string.Empty;
        public string ConfidenceCode { get; set; } = string.Empty;
        public bool IndicatesMissingRow { get; set; }
        public bool IndicatesTimedOut { get; set; }
        public bool IndicatesValueMismatch { get; set; }
        public bool IndicatesMissingColumn { get; set; }
        public bool IndicatesUniqueViolation { get; set; }
        public bool IndicatesForeignKeyViolation { get; set; }
        public bool IndicatesGeneratedColumnWrite { get; set; }
        public bool SupportsServerSettingProbe { get; set; }
        public List<LocationDto> ObjectReferences { get; set; } = new();
    }

    // islevi: Katalogda dogrulanmis sema, tablo, kolon ve constraint konumunu tasir.
    // sistemdeki gorevi: Dogrulanmamis provider adlarinin API cevabina sizmasini engelleyen public konum sozlesmesidir.
    public sealed class LocationDto
    {
        public string? SchemaName { get; set; }
        public string? TableName { get; set; }
        public string? ColumnName { get; set; }
        public string? ConstraintName { get; set; }
        public bool IsCatalogVerified { get; set; }
    }

    // islevi: Tek hipotezin kod, oncelik, guven, lokalize metin, kanit ve next-check alanlarini tasir.
    // sistemdeki gorevi: Confirmed-Likely-Possible-RuledOut siralamasini aciklanabilir ve makine-okur hale getirir.
    public sealed class HypothesisDto
    {
        public string HypothesisKindCode { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string ConfidenceCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public List<EvidenceDto> Evidence { get; set; } = new();
        public List<string> NextChecks { get; set; } = new();
    }

    // islevi: Probe turu, hipotez, sonuc olgusu, sayim ve redaction uygulanmis degerleri tasir.
    // sistemdeki gorevi: Hipotez basina en fazla uc yapilandirilmis kanitin API temsilidir.
    public sealed class EvidenceDto
    {
        public string ProbeKindCode { get; set; } = string.Empty;
        public string HypothesisKindCode { get; set; } = string.Empty;
        public string FactCode { get; set; } = string.Empty;
        public long? ObservedCount { get; set; }
        public string? ExpectedValue { get; set; }
        public string? ObservedValue { get; set; }
    }
}
