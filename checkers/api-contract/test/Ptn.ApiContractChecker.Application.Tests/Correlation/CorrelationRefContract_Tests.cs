using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ptn.ApiContractChecker.Dtos.Correlation;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Correlation;

// islevi: Public korelasyon DTO'sunun alan, tip ve JSON adlarini sabit ikiz sozlesmeye karsi dogrular.
// sistemdeki gorevi: Iki checker arasindaki 1:1 adapter varsayiminin sessizce bozulmasini engeller.
public class CorrelationRefContract_Tests
{
    [Fact]
    public void Contract_Should_Expose_Only_The_Twin_Fields_And_Json_Names()
    {
        var properties = typeof(CorrelationRefDto).GetProperties(BindingFlags.Instance | BindingFlags.Public);

        properties.Select(property => property.Name)
            .ShouldBe([nameof(CorrelationRefDto.TraceId), nameof(CorrelationRefDto.StepKey)], ignoreOrder: true);
        properties.ShouldAllBe(property => property.PropertyType == typeof(string));
        properties.Select(ResolveJsonName)
            .ShouldBe(["traceId", "stepKey"], ignoreOrder: true);
    }

    // islevi: Acik attribute varsa onu, yoksa ortak camelCase tel politikasini JSON adi olarak cozer.
    private static string ResolveJsonName(PropertyInfo property)
        => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
           ?? JsonNamingPolicy.CamelCase.ConvertName(property.Name);
}
