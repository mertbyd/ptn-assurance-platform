namespace Ptn.DatabaseChecker.Models.Runs;

// islevi: Bulgu sayfasi varsayilanini, tavanini ve UTF-8 cevap butcesini tek calisma profilinde tasir.
// sistemdeki gorevi: Tenant-aware setting zinciri ile query manager arasindaki tipli limit sozlesmesidir.
/// <summary>
/// Bulgu sorgusunun tenant-aware sayfa ve cevap boyutu limitleri.
/// </summary>
public sealed class FindingQuerySettings
{
    /// <summary>Istek boyut belirtmediginde kullanilan sayfa boyutu.</summary>
    public int DefaultPageSize { get; init; }
    /// <summary>Tek istekte izin verilen azami kayit sayisi.</summary>
    public int MaxPageSize { get; init; }
    /// <summary>Tek cevap icin azami UTF-8 JSON byte sayisi.</summary>
    public int MaxResponseBytes { get; init; }
}
