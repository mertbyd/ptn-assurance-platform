namespace Ptn.ApiContractChecker.Entities.Lookups;

// islevi: Bir spec snapshot'inin Swagger veya OpenAPI belge formatini siniflandirir.
// sistemdeki gorevi: Snapshot FK'sini format okuyucu cozumlemesinde kullanilan kararli Code ile birlestirir.
public class SpecFormat : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected SpecFormat()
    {
    }

    // Ortak lookup alanlarini tek taban invariantindan kurar.
    public SpecFormat(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
