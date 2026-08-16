using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Definitions;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Connections;

// islevi: DatabaseConnection ad tekilliginin tenant ve host-kullanici gorunurluk kapsamiyla ayni oldugunu dogrular.
// sistemdeki gorevi: Ortak SaaS hostunda farkli kullanicilarin birbirinin baglanti adlarini bloke etmesini engeller.
public class DatabaseConnectionVisibilityIndex_Tests
{
    [Fact]
    public void Connection_Unique_Indexes_Should_Match_Visibility_Scope()
    {
        var options = new DbContextOptionsBuilder<DatabaseCheckerDbContext>()
            .UseNpgsql("Host=localhost;Database=model-verification")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var dbContext = new DatabaseCheckerDbContext(options);
        // Provider-ozel migration annotation'lari optimize runtime modelinden budanabildigi icin design-time modeli okunur.
        var model = dbContext.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(DatabaseConnection)).ShouldNotBeNull();

        var tenantIndex = entityType.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(DatabaseConnection.TenantId), nameof(DatabaseConnection.Name) }));
        tenantIndex.IsUnique.ShouldBeTrue();
        tenantIndex.GetFilter().ShouldBe("\"TenantId\" IS NOT NULL");

        var hostUserIndex = entityType.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(DatabaseConnection.CreatorId), nameof(DatabaseConnection.Name) }));
        hostUserIndex.IsUnique.ShouldBeTrue();
        hostUserIndex.GetFilter().ShouldBe("\"TenantId\" IS NULL");
        hostUserIndex.GetAreNullsDistinct().ShouldBe(false);
    }

    [Fact]
    public void Definition_Unique_Indexes_Should_Match_Visibility_Scope()
    {
        var options = new DbContextOptionsBuilder<DatabaseCheckerDbContext>()
            .UseNpgsql("Host=localhost;Database=model-verification")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var dbContext = new DatabaseCheckerDbContext(options);
        var model = dbContext.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(ComparisonDefinition)).ShouldNotBeNull();

        var tenantIndex = entityType.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(ComparisonDefinition.TenantId), nameof(ComparisonDefinition.Name) }));
        tenantIndex.IsUnique.ShouldBeTrue();
        tenantIndex.GetFilter().ShouldBe("\"TenantId\" IS NOT NULL");

        var hostUserIndex = entityType.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(ComparisonDefinition.CreatorId), nameof(ComparisonDefinition.Name) }));
        hostUserIndex.IsUnique.ShouldBeTrue();
        hostUserIndex.GetFilter().ShouldBe("\"TenantId\" IS NULL");
        hostUserIndex.GetAreNullsDistinct().ShouldBe(false);
    }
}
