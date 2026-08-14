using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Ptn.TestModule.EntityFrameworkCore;
using Ptn.TestModule.Migrations;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Lookups;

// islevi: Uretilen modelin yalniz Test Module'un uc semasina dokundugunu dogrular.
// sistemdeki gorevi: Migration'in Auth, Notification veya checker tablolarini ikinci kez uretmesini engeller (RULE-0002, ADR-0016 §A).
/// <summary>
/// Test Module EF modeli ve son migration'in sema sahipligi testleridir.
/// </summary>
[Collection(TestModuleSchemaCollection.Name)]
public class MigrationScopeTests
{
    // Migration bu modelden uretildigi icin modeldeki sema kumesi migration kapsamiyla ayni olur.
    /// <summary>EF modelinin yalniz Test Module tarafindan sahiplenilen semalara map edildigini dogrular.</summary>
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

    // KBP-93 migration'inin yalniz test_run semasinda nesne olusturdugunu operation seviyesinde dogrular.
    /// <summary>TestRunRecords migration hedeflerinin yalniz test_run semasinda kaldigini dogrular.</summary>
    [Fact]
    public void Should_only_create_test_run_schema_objects_in_latest_migration()
    {
        var migrationBuilder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");
        var upMethod = typeof(TestRunRecords).GetMethod(
            "Up",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        upMethod.Invoke(new TestRunRecords(), [migrationBuilder]);

        var targetSchemas = migrationBuilder.Operations
            .Select(GetTargetSchema)
            .Where(schema => schema is not null)
            .Distinct()
            .ToList();
        var createdTables = migrationBuilder.Operations
            .OfType<CreateTableOperation>()
            .Select(operation => operation.Name)
            .OrderBy(name => name)
            .ToList();

        targetSchemas.ShouldBe([TestModuleDbProperties.RunSchema]);
        createdTables.ShouldBe([
            "test_result_findings",
            "test_run_results",
            "test_runs"
        ]);
    }

    // Modeldeki her entity tipinin sema adini toplar; sema atanmamis tip kalmamalidir.
    /// <summary>EF modelinde kullanilan kararli sema adlarini toplar.</summary>
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

    // Migration operasyonunun degistirdigi hedef semayi ortak bir metne indirger.
    /// <summary>Desteklenen migration operasyonunun hedef sema adini dondurur.</summary>
    private static string? GetTargetSchema(MigrationOperation operation)
    {
        return operation switch
        {
            EnsureSchemaOperation ensureSchema => ensureSchema.Name,
            CreateTableOperation createTable => createTable.Schema,
            CreateIndexOperation createIndex => createIndex.Schema,
            AddForeignKeyOperation addForeignKey => addForeignKey.Schema,
            _ => null
        };
    }
}
