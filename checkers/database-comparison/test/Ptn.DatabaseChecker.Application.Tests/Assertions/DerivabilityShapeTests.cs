using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Dtos.Projections;
using Ptn.DatabaseChecker.FluentValidation.Assertions;
using Ptn.DatabaseChecker.FluentValidation.Projections;
using Shouldly;
using System.Text.Json;
using Xunit;

namespace Ptn.DatabaseChecker.Application.Tests.Assertions;

// islevi: DB derivability output'unun adres+outcome seklini ve iki yeni public input validator kapisini dogrular.
// sistemdeki gorevi: API checker item'iyle hizali response'un girdi alanlariyla genislememesini ve bos isteklerin reddini korur.
public class DerivabilityShapeTests
{
    [Fact]
    public void Derivability_Item_Should_Contain_Only_Database_References_And_Outcome()
    {
        var properties = typeof(DerivabilityItemDto).GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ToArray();

        properties.ShouldBe(["ColumnRef", "OutcomeCode", "TableRef"]);
    }

    [Fact]
    public void Empty_New_Public_Inputs_Should_Be_Rejected()
    {
        new DerivabilityRequestDtoValidator()
            .Validate(new DerivabilityRequestDto())
            .IsValid.ShouldBeFalse();
        new ProjectionRequestDtoValidator()
            .Validate(new ProjectionRequestDto())
            .IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Projection_Rows_Should_Serialize_As_Dictionaries_Without_A_Wrapper()
    {
        var row = new ProjectionRowDto { ["email"] = "redacted" };
        var json = JsonSerializer.Serialize(new ProjectionResultDto { Rows = [row] });
        using var document = JsonDocument.Parse(json);
        var firstRow = document.RootElement.GetProperty("Rows")[0];

        firstRow.GetProperty("email").GetString().ShouldBe("redacted");
        firstRow.TryGetProperty("Values", out _).ShouldBeFalse();
    }
}
