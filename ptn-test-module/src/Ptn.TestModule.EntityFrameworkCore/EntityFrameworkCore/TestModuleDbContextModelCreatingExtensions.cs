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

        /* Entity konfigurasyonlari buraya eklenir (ADR-0016 §B).
         * Sema secimi: lookup -> LookupSchema, tanim -> CatalogSchema, kosum -> RunSchema.
         *
         * builder.Entity<Scenario>(b =>
         * {
         *     b.ToTable(TestModuleDbProperties.DbTablePrefix + "scenarios", TestModuleDbProperties.CatalogSchema);
         *     b.ConfigureByConvention();
         * });
         */
    }
}
