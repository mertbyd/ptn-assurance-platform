namespace Ptn.ApiContractChecker.Entities.Lookups;

// islevi: Bir contract farkinin request, response, endpoint veya dokumantasyon yonunu siniflandirir.
// sistemdeki gorevi: Engine bulgularinin direction Code degerleri icin yonetilebilir sistem sozlugudur.
public class DifferenceDirection : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected DifferenceDirection()
    {
    }

    // Ortak lookup alanlarini tek taban invariantindan kurar.
    public DifferenceDirection(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
