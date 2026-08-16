using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.Connections;

// islevi: DatabaseConnection tablosunun sema, kolon, index ve FK eslemelerini tanimlar.
// sistemdeki gorevi: Motor lookup FK'sini ve kiraci icinde benzersiz baglanti adi kuralini veritabani seviyesine tasir.
public class DatabaseConnectionConfiguration : IEntityTypeConfiguration<DatabaseConnection>
{
    public void Configure(EntityTypeBuilder<DatabaseConnection> builder)
    {
        builder.ToTable(DatabaseCheckerTableNames.DatabaseConnections, DatabaseCheckerDbProperties.ConnectionsSchema);
        builder.ConfigureByConvention();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(DatabaseConnectionConsts.MaxNameLength);
        builder.Property(x => x.Host).IsRequired().HasMaxLength(DatabaseConnectionConsts.MaxHostLength);
        builder.Property(x => x.DatabaseName).IsRequired().HasMaxLength(DatabaseConnectionConsts.MaxDatabaseNameLength);
        builder.Property(x => x.VaultSecretPath).IsRequired().HasMaxLength(DatabaseConnectionConsts.MaxVaultSecretPathLength);
        builder.Property(x => x.TlsModeCode)
            .IsRequired()
            .HasMaxLength(DatabaseConnectionConsts.MaxTlsModeCodeLength)
            .HasDefaultValue(TlsModeCodes.Require);
        builder.Property(x => x.TrustServerCertificate).HasDefaultValue(false);

        builder.HasOne(x => x.Engine)
            .WithMany()
            .HasForeignKey(x => x.EngineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(connection => new { connection.TenantId, connection.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL");

        builder.HasIndex(connection => new { connection.CreatorId, connection.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL")
            .AreNullsDistinct(false);
    }
}
