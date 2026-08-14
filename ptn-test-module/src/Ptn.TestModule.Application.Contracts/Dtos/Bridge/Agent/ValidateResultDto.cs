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
    /// Assertion turetilebilirlik kararini tasir.
    /// </summary>
    public DerivabilityResultDto? Derivability { get; set; }
    /// <summary>
    /// Assertion turetilebilirlik kararini tasir.
    /// </summary>
    public DatabaseDerivabilityResultDto? DatabaseDerivability { get; set; }
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<ClosedQuestionDto> Questions { get; set; } = [];
    /// <summary>
    /// Ayrintili kaynaga erisim adresini belirtir.
    /// </summary>
    public string? ResourceLink { get; set; }
}
