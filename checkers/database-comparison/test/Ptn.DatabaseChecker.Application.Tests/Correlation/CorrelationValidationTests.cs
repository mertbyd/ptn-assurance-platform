using System;
using System.Collections.Generic;
using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Dtos.Correlation;
using Ptn.DatabaseChecker.Dtos.Diagnosis;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.FluentValidation.Assertions;
using Ptn.DatabaseChecker.FluentValidation.Correlation;
using Ptn.DatabaseChecker.FluentValidation.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Correlation;

// islevi: Korelasyon trace ve adim kimligi sinirlarini ve root request validator baglantilarini dogrular.
// sistemdeki gorevi: Gecersiz korelasyonun assertion veya teshis manager'ina ulasmadan ayni kararli kodlarla reddedilmesini korur.
public sealed class CorrelationValidationTests
{
    // islevi: Trace uzunlugu/casing ve StepKey bosluk/uzunluk ihlallerini beklenen validation koduna baglar.
    [Theory]
    [MemberData(nameof(InvalidCorrelations))]
    public void Invalid_Correlation_Should_Return_The_Expected_Code(
        CorrelationRefDto input,
        string expectedCode)
    {
        var result = new CorrelationRefDtoValidator().Validate(input);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage == expectedCode);
    }

    // islevi: Assertion root validator'inin nested correlation kurallarini calistirdigini dogrular.
    [Fact]
    public void Assertion_Request_Should_Validate_Nested_Correlation()
    {
        var input = CreateValidAssertionRequest();
        input.Correlation = new CorrelationRefDto { TraceId = new string('A', 32) };

        var result = new RowAssertionRequestDtoValidator().Validate(input);

        result.Errors.ShouldContain(error =>
            error.PropertyName == "Correlation.TraceId" &&
            error.ErrorMessage == AssertionExceptionCodes.Validation.CorrelationTraceIdInvalid);
    }

    // islevi: Diagnosis root validator'inin nested correlation kurallarini calistirdigini dogrular.
    [Fact]
    public void Diagnosis_Request_Should_Validate_Nested_Correlation()
    {
        var input = CreateValidDiagnosisRequest();
        input.Correlation = new CorrelationRefDto { StepKey = " " };

        var result = new DiagnoseRequestDtoValidator().Validate(input);

        result.Errors.ShouldContain(error =>
            error.PropertyName == "Correlation.StepKey" &&
            error.ErrorMessage == AssertionExceptionCodes.Validation.CorrelationStepKeyInvalid);
    }

    // islevi: Sinir testlerinin kullandigi gecersiz correlation ve kararli hata kodu ciftlerini verir.
    public static IEnumerable<object[]> InvalidCorrelations()
    {
        yield return [new CorrelationRefDto { TraceId = new string('a', 31) },
            AssertionExceptionCodes.Validation.CorrelationTraceIdInvalid];
        yield return [new CorrelationRefDto { TraceId = new string('a', 33) },
            AssertionExceptionCodes.Validation.CorrelationTraceIdInvalid];
        yield return [new CorrelationRefDto { TraceId = new string('A', 32) },
            AssertionExceptionCodes.Validation.CorrelationTraceIdInvalid];
        yield return [new CorrelationRefDto { StepKey = string.Empty },
            AssertionExceptionCodes.Validation.CorrelationStepKeyInvalid];
        yield return [new CorrelationRefDto { StepKey = " " },
            AssertionExceptionCodes.Validation.CorrelationStepKeyInvalid];
        yield return [new CorrelationRefDto { StepKey = new string('s', 129) },
            AssertionExceptionCodes.Validation.CorrelationStepKeyInvalid];
    }

    // islevi: Nested validator testi icin gecerli adres, anahtar ve cardinality tasiyan assertion request'i kurar.
    private static RowAssertionRequestDto CreateValidAssertionRequest()
        => new()
        {
            ConnectionId = Guid.NewGuid(),
            SchemaName = "public",
            TableName = "orders",
            KeyValues = new Dictionary<string, string?> { ["id"] = "42" },
            Cardinality = new CardinalityExpectationDto
            {
                KindCode = CardinalityKindCodes.Exactly,
                ExpectedCount = 1
            }
        };

    // islevi: Nested validator testi icin exactly-one assertion sinyalli gecerli diagnosis request'i kurar.
    private static DiagnoseRequestDto CreateValidDiagnosisRequest()
        => new()
        {
            ConnectionId = Guid.NewGuid(),
            Assertion = new DiagnoseRequestDto.AssertionSignalDto
            {
                SchemaName = "public",
                TableName = "orders",
                OutcomeCode = AssertionOutcomeCodes.RowNotFound,
                KeyValues = new Dictionary<string, string?> { ["id"] = "42" }
            }
        };
}
