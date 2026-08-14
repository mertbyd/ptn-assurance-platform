using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace Ptn.TestModule.EntityFrameworkCore;

// islevi: Test Module entity konfigurasyonlarinin tek giris noktasidir.
// sistemdeki gorevi: Her entity kendi seed semasina baglanir; sema adi daima TestModuleDbProperties'ten okunur.
public static class TestModuleDbContextModelCreatingExtensions
{
    public static void ConfigureTestModule(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        /* Entity konfigurasyonlari IEntityTypeConfiguration olarak Configurations/ altinda yasar (ADR-0016 §B).
         * Sema secimi her configuration'in kendi tabaninda yapilir:
         * lookup -> LookupSchema, tanim -> CatalogSchema, kosum -> RunSchema.
         */
        builder.ApplyConfigurationsFromAssembly(typeof(TestModuleDbContextModelCreatingExtensions).Assembly);
    }
}
