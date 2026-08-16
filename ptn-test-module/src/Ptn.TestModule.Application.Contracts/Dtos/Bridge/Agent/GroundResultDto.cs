using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Dtos.Bridge.Database;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_ground kararini, kritik olguyu ve birlesik agir govdeyi tek cevapta tasir.
// sistemdeki gorevi: Kapsam ve karari response basinda, ayrintiyi inline veya ResourceLink arkasinda sunar.
public sealed class GroundResultDto
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
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string DecisionCode { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string CriticalFactCode { get; set; } = string.Empty;
    /// <summary>
    /// Secilen veya onerilen API operasyon eslesmesini tasir.
    /// </summary>
    public OperationBindingDto? OperationBinding { get; set; }
    /// <summary>
    /// Secilen operasyon icin uretilen ornek istegi tasir.
    /// </summary>
    public RequestExampleDto? RequestExample { get; set; }
    /// <summary>
    /// Sozlesmenin table description bilgisini belirtir.
    /// </summary>
    public TableDescriptionDto? TableDescription { get; set; }
    /// <summary>
    /// Yazma kumesi capability ve etki ozetini tasir.
    /// </summary>
    public FootprintResultDto Footprint { get; set; } = new();
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<ClosedQuestionDto> Questions { get; set; } = [];
    /// <summary>
    /// Ayrintili kaynaga erisim adresini belirtir.
    /// </summary>
    public string? ResourceLink { get; set; }
    /// <summary>
    /// Surdurulen yazarlik oturumunun kimligini belirtir; oturum verilmediyse bostur.
    /// </summary>
    public Guid? SessionId { get; set; }
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int StepCount { get; set; }
    /// <summary>
    /// Kontrollu sozlukteki ilgili kodlari kararli sirada listeler.
    /// </summary>
    public List<string> PendingQuestionCodes { get; set; } = [];
}
