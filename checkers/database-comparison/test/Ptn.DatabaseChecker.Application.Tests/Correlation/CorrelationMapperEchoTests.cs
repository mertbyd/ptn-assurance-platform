using Ptn.DatabaseChecker.Application.Mappers.Assertions;
using Ptn.DatabaseChecker.Application.Mappers.Diagnosis;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Dtos.Correlation;
using Ptn.DatabaseChecker.Dtos.Diagnosis;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Correlation;

// islevi: Mapperly'nin public DTO ve domain korelasyon modelleri arasinda iki yonlu kayipsiz echo uretebildigini dogrular.
// sistemdeki gorevi: Manager-owned correlation'in AppService'te elle property kopyalanmadan HTTP sonucuna ulasmasini korur.
public sealed class CorrelationMapperEchoTests
{
    // islevi: Assertion request correlation'inin domain result uzerinden public result DTO'suna ayni degerlerle tasindigini dogrular.
    [Fact]
    public void Assertion_Mapper_Should_Carry_Correlation_Through_The_Domain_Result()
    {
        var mapper = new DatabaseAssertionMapper();
        var request = mapper.MapToRequest(new RowAssertionRequestDto
        {
            Correlation = new CorrelationRefDto
            {
                TraceId = new string('a', 32),
                StepKey = "assertion-step"
            }
        });

        var result = mapper.MapToResultDto(new RowAssertionResult
        {
            Correlation = request.Correlation
        });

        result.Correlation.ShouldNotBeNull();
        result.Correlation.TraceId.ShouldBe(new string('a', 32));
        result.Correlation.StepKey.ShouldBe("assertion-step");
    }

    // islevi: Diagnosis request correlation'inin domain report uzerinden checknexus response DTO'suna tasindigini dogrular.
    [Fact]
    public void Diagnosis_Mapper_Should_Carry_Correlation_Through_The_Domain_Report()
    {
        var mapper = new DiagnosisMapper();
        var signal = mapper.MapToSignal(new DiagnoseRequestDto
        {
            Correlation = new CorrelationRefDto
            {
                TraceId = new string('b', 32),
                StepKey = "diagnosis-step"
            }
        });

        var result = mapper.MapToDto(new DiagnosisReport
        {
            Correlation = signal.Correlation
        });

        result.Correlation.ShouldNotBeNull();
        result.Correlation.TraceId.ShouldBe(new string('b', 32));
        result.Correlation.StepKey.ShouldBe("diagnosis-step");
    }

    // islevi: Correlation verilmediginde her iki mapper sonucunun da null contract'i korudugunu dogrular.
    [Fact]
    public void Mappers_Should_Keep_Null_Correlation()
    {
        var assertionResult = new DatabaseAssertionMapper().MapToResultDto(new RowAssertionResult());
        var diagnosisResult = new DiagnosisMapper().MapToDto(new DiagnosisReport());

        assertionResult.Correlation.ShouldBeNull();
        diagnosisResult.Correlation.ShouldBeNull();
    }
}
