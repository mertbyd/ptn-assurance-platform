namespace Ptn.ApiContractChecker.Entities.Lookups;

// islevi: Comparison engine'in uretebildigi atomik sozlesme fark turunu siniflandirir.
// sistemdeki gorevi: Owned bulgulardaki KindCode degerlerinin kapali ve yonetilebilir sistem sozlugudur.
public class DifferenceKind : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected DifferenceKind()
    {
    }

    // Ortak lookup alanlarini tek taban invariantindan kurar.
    public DifferenceKind(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
