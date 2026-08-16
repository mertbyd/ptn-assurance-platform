using FluentValidation.Results;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Dtos.Correlation;
using Ptn.ApiContractChecker.Dtos.Diagnosis;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.FluentValidation.Conformance;
using Ptn.ApiContractChecker.FluentValidation.Correlation;
using Ptn.ApiContractChecker.FluentValidation.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Correlation;

// islevi: Korelasyon sinirlarini ve uc public input validator'inin nested validation bagini dogrular.
// sistemdeki gorevi: Opsiyonelligi korurken gecersiz trace ve adim anahtarlarinin kararli kodla reddedilmesini saglar.
public class CorrelationValidation_Tests
{
    [Fact]
    public void Correlation_Should_Reject_Invalid_Trace_And_Step_Boundaries()
    {
        Validate(new CorrelationRefDto { TraceId = new string('a', 31) })
            .ShouldContain(failure => HasTraceCode(failure));
        Validate(new CorrelationRefDto { TraceId = new string('a', 33) })
            .ShouldContain(failure => HasTraceCode(failure));
        Validate(new CorrelationRefDto { TraceId = new string('A', 32) })
            .ShouldContain(failure => HasTraceCode(failure));
        Validate(new CorrelationRefDto { StepKey = "   " })
            .ShouldContain(failure => HasStepCode(failure));
        Validate(new CorrelationRefDto { StepKey = new string('s', 129) })
            .ShouldContain(failure => HasStepCode(failure));
    }

    [Fact]
    public void Public_Input_Validators_Should_Apply_The_Nested_Correlation_Contract()
    {
        var correlation = new CorrelationRefDto { TraceId = new string('A', 32) };

        new ResponseConformanceDtoValidator().Validate(new ResponseConformanceDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/items/1",
            StatusCode = 200,
            Correlation = correlation
        }).Errors.ShouldContain(failure => HasTraceCode(failure));
        new RequestConformanceDtoValidator().Validate(new RequestConformanceDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/items/1",
            Correlation = correlation
        }).Errors.ShouldContain(failure => HasTraceCode(failure));
        new DiagnoseRequestDtoValidator(new ProblemErrorDtoValidator()).Validate(new DiagnoseRequestDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/items/1",
            Correlation = correlation
        }).Errors.ShouldContain(failure => HasTraceCode(failure));
    }

    [Fact]
    public void Public_Input_Validators_Should_Keep_Missing_Correlation_Optional()
    {
        new ResponseConformanceDtoValidator().Validate(new ResponseConformanceDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/items/1",
            StatusCode = 200
        }).IsValid.ShouldBeTrue();
        new RequestConformanceDtoValidator().Validate(new RequestConformanceDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/items/1"
        }).IsValid.ShouldBeTrue();
        new DiagnoseRequestDtoValidator(new ProblemErrorDtoValidator()).Validate(new DiagnoseRequestDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/items/1"
        }).IsValid.ShouldBeTrue();
    }

    // islevi: Tek korelasyon girdisini asil validator ile calistirip hata listesini dondurur.
    private static List<ValidationFailure> Validate(CorrelationRefDto input)
        => new CorrelationRefDtoValidator().Validate(input).Errors;

    // islevi: Trace validation hatasinin kararli korelasyon kodunu tasidigini belirler.
    private static bool HasTraceCode(ValidationFailure failure)
        => failure.ErrorCode == GeneralExceptionCodes.CorrelationTraceIdInvalid;

    // islevi: StepKey validation hatasinin kararli korelasyon kodunu tasidigini belirler.
    private static bool HasStepCode(ValidationFailure failure)
        => failure.ErrorCode == GeneralExceptionCodes.CorrelationStepKeyInvalid;
}
