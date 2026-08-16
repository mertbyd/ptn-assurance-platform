using System;
using FluentValidation.TestHelper;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Dtos.Diagnosis;
using Ptn.DatabaseChecker.FluentValidation.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Diagnosis;

// islevi: Diagnose public DTO'sunun assertion ve database-exception kaynaklarindan tam olarak birini kabul ettigini dogrular.
// sistemdeki gorevi: Ambiguous sinyalin engine extractor'a ulasmadan FluentValidation sinirinda reddedilmesinin kontrat kanitidir.
public class DiagnoseRequestDtoValidator_Tests
{
    // islevi: Iki sinyal birden gelen ambiguous request'in exactly-one kuralinda reddedildigini dogrular.
    [Fact]
    public void Should_Reject_When_Both_Signals_Are_Present()
    {
        var input = CreateValidAssertionRequest();
        input.DbException = new DiagnoseRequestDto.DatabaseExceptionSignalDto
        {
            EngineCode = DatabaseEngineCodes.PostgreSql,
            SqlState = "23503"
        };

        var result = new DiagnoseRequestDtoValidator().TestValidate(input);

        result.ShouldHaveValidationErrorFor(item => item);
    }

    // islevi: Tek gecerli assertion sinyalinin public input validator'dan gectigini dogrular.
    [Fact]
    public void Should_Accept_One_Valid_Assertion_Signal()
    {
        var result = new DiagnoseRequestDtoValidator().Validate(CreateValidAssertionRequest());

        result.IsValid.ShouldBeTrue();
    }

    // islevi: Exactly-one testleri icin katalog adresi ve anahtari dolu gecerli assertion sinyali kurar.
    private static DiagnoseRequestDto CreateValidAssertionRequest()
        => new()
        {
            ConnectionId = Guid.NewGuid(),
            Assertion = new DiagnoseRequestDto.AssertionSignalDto
            {
                SchemaName = "public",
                TableName = "orders",
                OutcomeCode = AssertionOutcomeCodes.RowNotFound,
                KeyValues = { ["id"] = "42" }
            }
        };
}
