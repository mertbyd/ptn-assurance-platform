namespace Ptn.ApiContractChecker.Entities.Lookups;

// islevi: Bir contract farkinin kirici, kirici-olmayan veya dokumantasyon etkisini siniflandirir.
// sistemdeki gorevi: Engine bulgularinin severity Code degerleri icin yonetilebilir sistem sozlugudur.
public class DifferenceSeverity : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected DifferenceSeverity()
    {
    }

    // Ortak lookup alanlarini tek taban invariantindan kurar.
    public DifferenceSeverity(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
