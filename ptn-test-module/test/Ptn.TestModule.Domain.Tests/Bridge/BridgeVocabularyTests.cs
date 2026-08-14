using System.Linq;
using Ptn.TestModule.Models.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: Kopru modellerinde API ve DB sema anlamlarini yeniden birlestiren alan adlarini tarar.
// sistemdeki gorevi: SchemaName cakismasinin yeni bir modelle sessizce geri gelmesini engeller.
public class BridgeVocabularyTests
{
    // Tum kopru model property'lerinde yasakli SchemaName adini arar.
    [Fact]
    public void Should_not_expose_ambiguous_schema_name_property()
    {
        var bridgeTypes = typeof(PtnLocation).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(PtnLocation).Namespace);

        var ambiguousProperties = bridgeTypes
            .SelectMany(type => type.GetProperties())
            .Where(property => property.Name == "SchemaName")
            .Select(property => $"{property.DeclaringType?.Name}.{property.Name}")
            .ToList();

        ambiguousProperties.ShouldBeEmpty();
    }
}
