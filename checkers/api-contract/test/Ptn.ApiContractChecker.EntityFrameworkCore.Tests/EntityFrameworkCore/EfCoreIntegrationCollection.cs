using Xunit;

// EF integration assembly'sindeki inherited ABP testleri dahil tum siniflari deterministik olarak seri calistirir.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Ptn.ApiContractChecker.EntityFrameworkCore;

// islevi: ABP SQLite integrated test siniflarini ayni seri koleksiyonda toplar.
// sistemdeki gorevi: Her sinifin modul baslangic seed'i sirasinda paylasilan SQLite altyapisinin kilitlenmesini engeller.
[CollectionDefinition(Name, DisableParallelization = true)]
public class EfCoreIntegrationCollection
{
    public const string Name = "ApiContractChecker EF Core integration";
}
