using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ptn.TestModule.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Lookups;

// islevi: Uretilen modelin yalniz Test Module'un uc semasina dokundugunu dogrular.
// sistemdeki gorevi: Migration'in Auth, Notification veya checker tablolarini ikinci kez uretmesini engeller (RULE-0002, ADR-0016 §A).
[Collection(TestModuleSchemaCollection.Name)]
public class MigrationScopeTests
{
    // Migration bu modelden uretildigi icin modeldeki sema kumesi migration kapsamiyla ayni olur.
    [Fact]
    public void Should_only_map_test_module_owned_schemas()
    {
        var ownedSchemas = new[]
        {
            TestModuleDbProperties.LookupSchema,
            TestModuleDbProperties.CatalogSchema,
            TestModuleDbProperties.RunSchema
        };

        var mappedSchemas = GetMappedSchemas();

        mappedSchemas.ShouldNotBeEmpty();
        mappedSchemas.ShouldAllBe(schema => ownedSchemas.Contains(schema));
    }

    // Modeldeki her entity tipinin sema adini toplar; sema atanmamis tip kalmamalidir.
    private static IReadOnlyList<string> GetMappedSchemas()
    {
        var options = new DbContextOptionsBuilder<TestModuleDbContext>()
            .UseSqlite("DataSource=:memory:")
            .UseSnakeCaseNamingConvention()
            .Options;

        using var context = new TestModuleDbContext(options);

        return context.Model.GetEntityTypes()
            .Select(entityType => entityType.GetSchema())
            .Where(schema => !string.IsNullOrWhiteSpace(schema))
            .Distinct()
            .ToList()!;
    }
}
