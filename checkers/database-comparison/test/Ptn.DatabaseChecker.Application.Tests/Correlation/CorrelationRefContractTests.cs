using System.Linq;
using System.Reflection;
using System.Text.Json;
using Ptn.DatabaseChecker.Dtos.Correlation;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Correlation;

// islevi: CorrelationRefDto alan, nullability ve JSON adlarini kararli ikiz sozlesmesine karsi dogrular.
// sistemdeki gorevi: API ve Database checker korelasyon tiplerinin zamanla farkli tel sekillerine kaymasini engeller.
public sealed class CorrelationRefContractTests
{
    // islevi: Public alan kumesi, nullable string tipleri ve camel-case JSON adlarinin birebir beklenen oldugunu dogrular.
    [Fact]
    public void Contract_Should_Contain_Only_The_Expected_Nullable_String_Properties()
    {
        var properties = typeof(CorrelationRefDto)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .OrderBy(property => property.Name)
            .ToArray();
        var nullability = new NullabilityInfoContext();

        properties.Select(property => property.Name).ShouldBe(["StepKey", "TraceId"]);
        properties.ShouldAllBe(property => property.PropertyType == typeof(string));
        properties.ShouldAllBe(property =>
            nullability.Create(property).ReadState == NullabilityState.Nullable);

        using var document = JsonSerializer.SerializeToDocument(
            new CorrelationRefDto { TraceId = new string('a', 32), StepKey = "step-1" },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        document.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ShouldBe(["stepKey", "traceId"]);
    }
}
