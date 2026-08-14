using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Api;
using Ptn.TestModule.Models.Bridge.Database;
using Ptn.TestModule.Models.Bridge.Diagnosis;
using BridgeDiagnosisReportDto = Ptn.TestModule.Dtos.Bridge.Diagnosis.DiagnosisReportDto;
using BridgeDiagnosisRequestDto = Ptn.TestModule.Dtos.Bridge.Diagnosis.DiagnosisRequestDto;
using BridgeCorrelationRefDto = Ptn.TestModule.Dtos.Bridge.CorrelationRefDto;
using Riok.Mapperly.Abstractions;
using ApiDiagnoseRequestDto = Ptn.ApiContractChecker.Dtos.Diagnosis.DiagnoseRequestDto;
using ApiDiagnosisReportDto = Ptn.ApiContractChecker.Dtos.Diagnosis.DiagnosisReportDto;
using ApiEvidenceDto = Ptn.ApiContractChecker.Dtos.Diagnosis.ProbeEvidenceDto;
using ApiHypothesisDto = Ptn.ApiContractChecker.Dtos.Diagnosis.HypothesisAssessmentDto;
using ApiIdentityDto = Ptn.ApiContractChecker.Dtos.Diagnosis.FailureIdentityDto;
using ApiProblemErrorDto = Ptn.ApiContractChecker.Dtos.Diagnosis.ProblemErrorDto;
using DatabaseDiagnoseRequestDto = Ptn.DatabaseChecker.Dtos.Diagnosis.DiagnoseRequestDto;
using DatabaseDiagnosisReportDto = Ptn.DatabaseChecker.Dtos.Diagnosis.DiagnosisReportDto;
using DatabaseEvidenceDto = Ptn.DatabaseChecker.Dtos.Diagnosis.DiagnosisReportDto.EvidenceDto;
using DatabaseHypothesisDto = Ptn.DatabaseChecker.Dtos.Diagnosis.DiagnosisReportDto.HypothesisDto;
using ApiCorrelationRefDto = Ptn.ApiContractChecker.Dtos.Correlation.CorrelationRefDto;
using DatabaseCorrelationRefDto = Ptn.DatabaseChecker.Dtos.Correlation.CorrelationRefDto;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Iki checker diagnosis DTO ailesi ile Bridge modelleri arasindaki compile-time eslemeleri uretir.
// sistemdeki gorevi: Mapper dosyasini yalniz saf Mapperly imzalarinda tutup semantik kurallari servise birakir.
[Mapper]
public partial class FailureDiagnosisMapper
{
    public partial DiagnosisRequest Map(BridgeDiagnosisRequestDto input);
    public partial BridgeDiagnosisReportDto Map(DiagnosisReport input);
    public partial ApiDiagnoseRequestDto Map(ApiDiagnosisRequest input);
    public partial DatabaseDiagnoseRequestDto Map(DatabaseDiagnosisRequest input);
    public partial ApiDiagnosisReportSource Map(ApiDiagnosisReportDto input);
    public partial DatabaseDiagnosisReportSource Map(DatabaseDiagnosisReportDto input);
    public partial ApiFailureIdentity Map(ApiIdentityDto input);
    public partial ApiProblemErrorDto Map(ProblemError input);
    public partial DatabaseDiagnoseRequestDto.AssertionSignalDto Map(DatabaseAssertionSignal input);
    public partial DatabaseDiagnoseRequestDto.DatabaseExceptionSignalDto Map(DatabaseExceptionSignal input);
    public partial Ptn.DatabaseChecker.Dtos.Assertions.FailedExpectationDto Map(FailedExpectation input);
    private partial ApiDiagnosisHypothesis Map(ApiHypothesisDto input);
    private partial DatabaseDiagnosisHypothesis Map(DatabaseHypothesisDto input);
    private partial ApiDiagnosisEvidence Map(ApiEvidenceDto input);
    private partial DatabaseDiagnosisEvidence Map(DatabaseEvidenceDto input);
    private partial CorrelationRef Map(ApiCorrelationRefDto input);
    private partial CorrelationRef Map(DatabaseCorrelationRefDto input);
    private partial ApiCorrelationRefDto MapToApi(CorrelationRef input);
    private partial DatabaseCorrelationRefDto MapToDatabase(CorrelationRef input);
    private partial CorrelationRef Map(BridgeCorrelationRefDto input);
    private partial BridgeCorrelationRefDto MapToBridge(CorrelationRef input);
}
