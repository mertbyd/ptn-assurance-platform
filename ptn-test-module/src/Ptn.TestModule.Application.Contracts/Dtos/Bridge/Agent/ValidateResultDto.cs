using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Dtos.Bridge.Database;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_validate yayin karari, kapsam ve turetilebilirlik ayrintisini tasir.
// sistemdeki gorevi: Spec veya referans boslugunda kapali Inconclusive sonucunu Allow gibi gostermeyi engeller.
public sealed class ValidateResultDto
{
    /// <summary>
    /// Cevabin concise veya ayrintili sunum bicimini belirtir.
    /// </summary>
    public string ResponseFormat { get; set; } = string.Empty;
    /// <summary>
    /// Profilin kavram kapsama ozetini tasir.
    /// </summary>
    public CoverageReportDto Coverage { get; set; } = new();
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool IsPublishable { get; set; }
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string DecisionCode { get; set; } = string.Empty;
    /// <summary>
    /// Redocly lint ile dogrulanan Arazzo sema sonucunu belirtir.
    /// </summary>
    public bool IsSchemaValid { get; set; }
    /// <summary>
    /// Derleyicinin kaynak belgeden saydigi assertion toplamidir.
    /// </summary>
    public int AssertionCount { get; set; }
    /// <summary>
    /// Assertion turetilebilirlik kararini tasir.
    /// </summary>
    public DerivabilityResultDto? Derivability { get; set; }
    /// <summary>
    /// Assertion turetilebilirlik kararini tasir.
    /// </summary>
    public DatabaseDerivabilityResultDto? DatabaseDerivability { get; set; }
    /// <summary>
    /// Basarisiz mevcut yayin kapilarini kararli degerlendirme sirasinda listeler.
    /// </summary>
    public IReadOnlyList<string> FailedGateCodes { get; set; } = [];
    /// <summary>
    /// Yayin kararini dusurmeyen sema uyarilarini listeler.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; set; } = [];
    /// <summary>
    /// Redocly lint ciktisinin aciklayici metnini tasir.
    /// </summary>
    public string LintDiagnostics { get; set; } = string.Empty;
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<ClosedQuestionDto> Questions { get; set; } = [];
    /// <summary>
    /// Ayrintili kaynaga erisim adresini belirtir.
    /// </summary>
    public string? ResourceLink { get; set; }
}
