using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Ptn.TestModule.Constants;
using Ptn.TestModule.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Lookups;

// islevi: Uc semanin da configuration uzerinden ezilebildigini dogrular.
// sistemdeki gorevi: Sema adi koda gomulurse ortam bazli dagitim kirilir; bu test o gomulmeyi yakalar (RULE-0002, ADR-0016 §A).
[Collection(TestModuleSchemaCollection.Name)]
public class SchemaOwnershipTests
{
    // Configuration bolumu verildiginde uc sema da yeni degeri almalidir.
    [Fact]
    public void Should_override_all_three_schemas_from_configuration()
    {
        var originalLookup = TestModuleDbProperties.LookupSchema;
        var originalCatalog = TestModuleDbProperties.CatalogSchema;
        var originalRun = TestModuleDbProperties.RunSchema;

        try
        {
            var configuration = BuildSchemaConfiguration("other_lookup", "other_catalog", "other_run");

            TestModuleEntityFrameworkCoreModule.ConfigureSchemas(configuration);

            TestModuleDbProperties.LookupSchema.ShouldBe("other_lookup");
            TestModuleDbProperties.CatalogSchema.ShouldBe("other_catalog");
            TestModuleDbProperties.RunSchema.ShouldBe("other_run");
        }
        finally
        {
            TestModuleDbProperties.LookupSchema = originalLookup;
            TestModuleDbProperties.CatalogSchema = originalCatalog;
            TestModuleDbProperties.RunSchema = originalRun;
        }
    }

    // Bolum hic yoksa Domain.Shared varsayilanlari korunmalidir.
    [Fact]
    public void Should_keep_defaults_when_no_schema_section_exists()
    {
        var expectedLookup = TestModuleDbProperties.LookupSchema;

        TestModuleEntityFrameworkCoreModule.ConfigureSchemas(new ConfigurationBuilder().Build());

        TestModuleDbProperties.LookupSchema.ShouldBe(expectedLookup);
    }

    private static IConfiguration BuildSchemaConfiguration(string lookup, string catalog, string run)
    {
        var section = TestModuleConfigurationKeys.EntityFrameworkCoreSchemasSection;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{section}:{TestModuleConfigurationKeys.LookupSchema}"] = lookup,
                [$"{section}:{TestModuleConfigurationKeys.CatalogSchema}"] = catalog,
                [$"{section}:{TestModuleConfigurationKeys.RunSchema}"] = run
            })
            .Build();
    }
}
