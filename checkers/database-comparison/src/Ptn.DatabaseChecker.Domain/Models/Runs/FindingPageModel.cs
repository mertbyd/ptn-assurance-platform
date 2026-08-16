namespace Ptn.DatabaseChecker.Models.Runs;

// islevi: Filtreli bulgu toplam sayisi ile cikti butcesine sigan tek sayfayi tasir.
// sistemdeki gorevi: Domain sorgu manager'inin ABP DTO tipine baglanmadan sayfali sonuc uretmesini saglar.
/// <summary>
/// Filtreli bulgularin toplam sayisini ve tek sonuc sayfasini tasir.
/// </summary>
public sealed class FindingPageModel
{
    /// <summary>Sayfalama oncesindeki filtreli toplam bulgu sayisi.</summary>
    public long TotalCount { get; set; }
    /// <summary>Cikti butcesine sigan bulgu sayfasi.</summary>
    public List<FindingReadModel> Items { get; set; } = new();
}
