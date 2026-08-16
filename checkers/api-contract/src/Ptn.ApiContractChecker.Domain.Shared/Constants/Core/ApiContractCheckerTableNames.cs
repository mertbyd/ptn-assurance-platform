namespace Ptn.ApiContractChecker.Constants;

// islevi: Uygulama semasindaki tablo adlarini tek kaynakta toplar.
// sistemdeki gorevi: EF configuration dosyalarinda anlamli tablo adi string'lerinin dagilmasini engeller; migration, seed ve mapping ayni sabitleri kullanir.
public static class ApiContractCheckerTableNames
{
    // Lookup: okunabilen spec surumleri (Swagger 2.0 / OAS 3.0 / 3.1 / 3.2).
    public const string SpecFormats = "spec_formats";

    // Lookup: kontrol calistirmasinin yasam dongusu durumlari.
    public const string CheckRunStatuses = "check_run_statuses";

    // Lookup: farkin kirin kirmadigi (breaking / non-breaking / docs-only).
    public const string DifferenceSeverities = "difference_severities";

    // Lookup: farkin hangi yonu ilgilendirdigi (request / response / endpoint / documentation).
    public const string DifferenceDirections = "difference_directions";

    // Lookup: fark turu; kodlar oasdiff kural adlariyla hizalidir.
    public const string DifferenceKinds = "difference_kinds";

    // Izlenen servisler (taban adres + kasadaki kimlik yolu).
    public const string SpecSources = "spec_sources";

    // Bir kaynagin yayimladigi dokumanlar (v1, internal ...).
    public const string SpecDocuments = "spec_documents";

    // Indirilen ham spec icerigi; hash ile adreslenir ve bir kez saklanir.
    public const string SpecContents = "spec_contents";

    // Bir dokumanin belirli bir andaki anlik goruntusu.
    public const string SpecSnapshots = "spec_snapshots";

    // Iki anlik goruntunun karsilastirilmasindan dogan calistirma kaydi.
    public const string ContractCheckRuns = "contract_check_runs";
}
