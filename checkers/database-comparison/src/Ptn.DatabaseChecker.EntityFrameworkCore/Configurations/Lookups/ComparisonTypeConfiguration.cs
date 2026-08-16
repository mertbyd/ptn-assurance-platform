using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Lookups;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: ComparisonType lookup tablosunun sema/kolon/index eslemesini tanimlar.
// sistemdeki gorevi: comparison.comparison_types tablosunu, kolon uzunluklarini (LookupConsts) ve Code uzerindeki benzersiz index'i EF'e bildirir.
public class ComparisonTypeConfiguration : IEntityTypeConfiguration<ComparisonType>
{
    public void Configure(EntityTypeBuilder<ComparisonType> builder)
    {
        // Tablo comparison semasinda; ad cogul ve snake_case (proje konvansiyonu, bkz. database_engines).
        builder.ToTable(DatabaseCheckerTableNames.ComparisonTypes, DatabaseCheckerDbProperties.LookupsSchema);
        builder.ConfigureByConvention();
        // Kararli teknik anahtar; zorunlu ve sinirli uzunluk (LookupConsts tek kaynak).
        builder.Property(x => x.Code).IsRequired().HasMaxLength(LookupConsts.MaxCodeLength);
        // Insan-okur ad; zorunlu.
        builder.Property(x => x.Name).IsRequired().HasMaxLength(LookupConsts.MaxNameLength);
        // Opsiyonel aciklama.
        builder.Property(x => x.Description).HasMaxLength(LookupConsts.MaxDescriptionLength);
        // Ayni kod iki kez girilemez; FK cozumleme ve seed idempotansi bu index'e guvenir.
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
