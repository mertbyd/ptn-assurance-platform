using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.ApiContractChecker.Constants.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: Tum lookup entity'lerinin ortak kolon, sema ve kararli Code index eslemesini tanimlar.
// sistemdeki gorevi: Bes lookup configuration dosyasinda ayni EF davranisinin tekrar kurulmasini engeller.
public abstract class LookupEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : LookupEntity
{
    protected abstract string TableName { get; }

    // Ortak lookup modelini concrete tablonun adiyla checker semasina uygular.
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(TableName, ApiContractCheckerDbProperties.CheckerSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.Code)
            .IsRequired()
            .HasMaxLength(LookupConsts.MaxCodeLength);
        builder.Property(entity => entity.Name)
            .IsRequired()
            .HasMaxLength(LookupConsts.MaxNameLength);
        builder.Property(entity => entity.Description)
            .HasMaxLength(LookupConsts.MaxDescriptionLength);
        builder.Property(entity => entity.IsActive)
            .IsRequired();
        builder.HasIndex(entity => entity.Code)
            .IsUnique();
    }
}
