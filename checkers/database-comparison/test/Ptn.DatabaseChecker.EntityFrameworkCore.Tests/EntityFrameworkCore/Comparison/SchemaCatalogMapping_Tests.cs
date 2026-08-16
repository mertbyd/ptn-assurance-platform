using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery.PostgreSql;
using Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery.SqlServer;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;
using Ptn.DatabaseChecker.Repository.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Comparison;

// islevi: KBP-703 katalog alanlarinin gercek provider kolonlarina ve ortak constraint modeline baglandigini dogrular.
// sistemdeki gorevi: Ham SQL olmadan kurulan CatalogRow + Configuration + repository mapping zincirinin trust/collation/column-depth regresyonlarini korur.
public class SchemaCatalogMapping_Tests
{
    // islevi: PostgreSQL constraint trust alanlarinin pg_constraint kolonlarindan ortak modele tasindigini dogrular.
    [Fact]
    public void PostgreSql_Not_Valid_Constraint_Should_Map_As_Unvalidated()
    {
        using var dbContext = CreatePostgreSqlContext();
        GetColumnName<PostgreSqlConstraintCatalogRow>(dbContext.Model, nameof(PostgreSqlConstraintCatalogRow.IsValidated))
            .ShouldBe(DatabaseMetadataCatalogConstants.PostgreSql.ConValidatedColumn);
        var row = new PostgreSqlConstraintCatalogRow
        {
            Name = "FK_orders_customer",
            Type = DatabaseMetadataCatalogConstants.PostgreSql.ForeignKeyConType,
            IsValidated = false
        };

        var unvalidated = MapPostgreSqlConstraint(row);
        row.IsValidated = true;
        var validated = MapPostgreSqlConstraint(row);

        unvalidated.IsValidated.ShouldBeFalse();
        validated.IsValidated.ShouldBeTrue();
    }

    // islevi: SQL Server FK trust alanlarinin sys.foreign_keys kolonlarindan ortak modele tasindigini dogrular.
    [Fact]
    public void SqlServer_Nocheck_Constraint_Should_Map_As_Unvalidated()
    {
        using var dbContext = CreateSqlServerContext();
        GetColumnName<SqlServerForeignKeyCatalogRow>(dbContext.Model, nameof(SqlServerForeignKeyCatalogRow.IsNotTrusted))
            .ShouldBe(DatabaseMetadataCatalogConstants.SqlServer.IsNotTrustedColumn);
        var row = new SqlServerForeignKeyCatalogRow { Name = "FK_orders_customer", IsNotTrusted = true };

        var unvalidated = MapSqlServerForeignKey(row);
        row.IsNotTrusted = false;
        var validated = MapSqlServerForeignKey(row);

        unvalidated.IsValidated.ShouldBeFalse();
        validated.IsValidated.ShouldBeTrue();
    }

    // islevi: PostgreSQL collation/generated/comment/identity ve trigger alanlarinin beklenen katalog kolonlarina baglandigini dogrular.
    [Fact]
    public void PostgreSql_Depth_Fields_Should_Use_Catalog_Mappings()
    {
        using var dbContext = CreatePostgreSqlContext();
        GetColumnName<PostgreSqlAttributeCatalogRow>(dbContext.Model, nameof(PostgreSqlAttributeCatalogRow.Generated)).ShouldBe(DatabaseMetadataCatalogConstants.PostgreSql.AttGeneratedColumn);
        GetColumnName<PostgreSqlAttributeCatalogRow>(dbContext.Model, nameof(PostgreSqlAttributeCatalogRow.CollationId)).ShouldBe(DatabaseMetadataCatalogConstants.PostgreSql.AttCollationColumn);
        GetColumnName<PostgreSqlTriggerCatalogRow>(dbContext.Model, nameof(PostgreSqlTriggerCatalogRow.EnabledStatus)).ShouldBe(DatabaseMetadataCatalogConstants.PostgreSql.TgEnabledColumn);
        GetColumnName<PostgreSqlDescriptionCatalogRow>(dbContext.Model, nameof(PostgreSqlDescriptionCatalogRow.Description)).ShouldBe(DatabaseMetadataCatalogConstants.PostgreSql.DescriptionTextColumn);
        GetColumnName<PostgreSqlDependCatalogRow>(dbContext.Model, nameof(PostgreSqlDependCatalogRow.ReferencedObjectSubId)).ShouldBe(DatabaseMetadataCatalogConstants.PostgreSql.DependReferencedObjectSubIdColumn);
    }

    // islevi: SQL Server collation/computed/identity/comment/disabled alanlarinin beklenen katalog kolonlarina baglandigini dogrular.
    [Fact]
    public void SqlServer_Depth_Fields_Should_Use_Catalog_Mappings()
    {
        using var dbContext = CreateSqlServerContext();
        GetColumnName<SqlServerColumnCatalogRow>(dbContext.Model, nameof(SqlServerColumnCatalogRow.CollationName)).ShouldBe(DatabaseMetadataCatalogConstants.SqlServer.CollationNameColumn);
        GetColumnName<SqlServerComputedColumnCatalogRow>(dbContext.Model, nameof(SqlServerComputedColumnCatalogRow.Definition)).ShouldBe(DatabaseMetadataCatalogConstants.SqlServer.DefinitionColumn);
        GetColumnName<SqlServerIdentityColumnCatalogRow>(dbContext.Model, nameof(SqlServerIdentityColumnCatalogRow.SeedValue)).ShouldBe(DatabaseMetadataCatalogConstants.SqlServer.SeedValueColumn);
        GetColumnName<SqlServerExtendedPropertyCatalogRow>(dbContext.Model, nameof(SqlServerExtendedPropertyCatalogRow.Value)).ShouldBe(DatabaseMetadataCatalogConstants.SqlServer.ExtendedPropertyValueColumn);
        GetColumnName<SqlServerTriggerCatalogRow>(dbContext.Model, nameof(SqlServerTriggerCatalogRow.IsDisabled)).ShouldBe(DatabaseMetadataCatalogConstants.SqlServer.IsDisabledColumn);
    }

    // islevi: Test icin baglanti acmadan PostgreSQL katalog modelini kurar.
    private static PostgreSqlCatalogDbContext CreatePostgreSqlContext()
        => new(new DbContextOptionsBuilder<PostgreSqlCatalogDbContext>()
            .UseNpgsql("Host=localhost;Database=catalog_mapping;Username=test;Password=test")
            .Options);

    // islevi: Test icin baglanti acmadan SQL Server katalog modelini kurar.
    private static SqlServerCatalogDbContext CreateSqlServerContext()
        => new(new DbContextOptionsBuilder<SqlServerCatalogDbContext>()
            .UseSqlServer("Server=localhost;Database=catalog_mapping;User Id=test;Password=test;TrustServerCertificate=True")
            .Options);

    // islevi: PostgreSQL constraint katalog satirini bos referans sozlukleriyle ortak modele cevirir.
    private static Models.Comparison.SchemaConstraintModel MapPostgreSqlConstraint(PostgreSqlConstraintCatalogRow row)
        => PostgreSqlDatabaseSchemaDiscoveryRepository.MapConstraint(row, null, new(), new());

    // islevi: SQL Server FK katalog satirini bos kolon/referans sozlukleriyle ortak modele cevirir.
    private static Models.Comparison.SchemaConstraintModel MapSqlServerForeignKey(SqlServerForeignKeyCatalogRow row)
        => SqlServerDatabaseSchemaDiscoveryRepository.MapForeignKeyConstraint(row, new(), new(), new());

    // islevi: Verilen catalog row property sinin provider store kolon adini EF modelinden okur.
    private static string GetColumnName<TEntity>(IModel model, string propertyName)
    {
        var entityType = model.FindEntityType(typeof(TEntity))!;
        var storeObject = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        return entityType.FindProperty(propertyName)!.GetColumnName(storeObject)!;
    }
}
