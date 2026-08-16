using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace Ptn.ApiContractChecker.EntityFrameworkCore;

// islevi: Uygulamanin kendi EF yapilandirmalarini model olusturma asamasinda tek noktadan uygular.
// sistemdeki gorevi: Her entity icin DbContext icinde elle cagri yazilmasini engeller; Configurations/ klasoru otomatik taranir.
public static class ApiContractCheckerDbContextModelCreatingExtensions
{
    // islevi: Configurations/ altindaki tum IEntityTypeConfiguration<> siniflarini modele uygular.
    public static void ConfigureApiContractChecker(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // IEntityTypeConfiguration<> dosyalarini (Configurations/ klasoru) otomatik tarar ve uygular.
        // Migration DbContext bu metodu cagirdigi icin entity'ler migration'a dahil olur.
        builder.ApplyConfigurationsFromAssembly(
            typeof(ApiContractCheckerDbContextModelCreatingExtensions).Assembly);
    }
}
