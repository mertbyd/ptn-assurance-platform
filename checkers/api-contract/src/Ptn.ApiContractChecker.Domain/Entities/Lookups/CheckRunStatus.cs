namespace Ptn.ApiContractChecker.Entities.Lookups;

// islevi: Contract check run yasam dongusunun kalici durumlarini siniflandirir.
// sistemdeki gorevi: Run durum FK'sini Pending, Running ve terminal kararli kodlara baglar.
public class CheckRunStatus : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected CheckRunStatus()
    {
    }

    // Ortak lookup alanlarini tek taban invariantindan kurar.
    public CheckRunStatus(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive)
    {
    }
}
