using Ptn.DatabaseChecker.Constants.Comparison;
using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Dtos.Runs;

// islevi: MCP'nin kalici run bulgularini siddet, tur ve adresle filtreleyerek sayfali okuma girdisidir.
// sistemdeki gorevi: ABP standart SkipCount/MaxResultCount kontratini bulgu filtreleriyle genisletir.
/// <summary>
/// Kalici run bulgulari icin ABP sayfalama ve opsiyonel filtre girdisi.
/// </summary>
public class FindingQueryInput : PagedResultRequestDto
{
    /// <summary>
    /// Isteği varsayilan 20 kayitlik sayfayla baslatir.
    /// </summary>
    public FindingQueryInput()
    {
        MaxResultCount = ComparisonRunConsts.DefaultFindingPageSize;
    }

    /// <summary>
    /// Opsiyonel Breaking, NonBreaking, Warning veya DocsOnly filtresi.
    /// </summary>
    public string? SeverityCode { get; set; }

    /// <summary>
    /// Opsiyonel OnlyInSource, OnlyInTarget veya Modified filtresi.
    /// </summary>
    public string? KindCode { get; set; }

    /// <summary>
    /// Opsiyonel sema nesne turu filtresi.
    /// </summary>
    public string? ObjectTypeCode { get; set; }

    /// <summary>
    /// Opsiyonel sema adi filtresi.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Opsiyonel tablo adi filtresi.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>Ayni definition icindeki daha eski Completed run'a gore yeni bulgulari secer.</summary>
    public Guid? SinceRunId { get; set; }

    /// <summary>Opsiyonel, sinirli SHA-256 fingerprint altkumesi; bos liste filtre uygulamaz.</summary>
    public List<string> Fingerprints { get; set; } = [];
}
