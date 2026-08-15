namespace Ptn.TestModule.Constants.Catalog;

// islevi: Kapsam raporunun rota, Swagger grubu ve bilinmeyen payda gerekce kodunu tanimlar.
// sistemdeki gorevi: Paydanin neden bilinmedigini metin degil kararli kod olarak tasir (KBP-110 §2.2).
/// <summary>
/// Senaryo kapsam raporunun kararli adres ve gerekce sabitlerini tasir.
/// </summary>
public static class ScenarioCoverageConsts
{
    /// <summary>Kapsam raporunun kok rotasidir.</summary>
    public const string Root = "api/test-module/coverage";

    /// <summary>Kapsam raporu endpoint'inin Swagger grup adidir.</summary>
    public const string SwaggerGroupName = "test-module-coverage";

    /// <summary>Snapshot operasyon envanteri checker'da acilmadigi icin payda bilinmiyordur.</summary>
    public const string DenominatorUnknownReason = "TestModule.Coverage:SnapshotOperationInventoryUnavailable";

    /// <summary>Payda bilinmedigi surece raporda tasinan kararli durum kodudur.</summary>
    public const string DenominatorUnknownState = "Unknown";

    /// <summary>Tek raporda taranan azami yayinlanmis senaryo sayisidir.</summary>
    public const int MaxScenariosPerReport = 1_000;
}
