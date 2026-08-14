using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Api;
using Ptn.TestModule.Models.Bridge.Database;
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

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Iki checker diagnosis DTO ailesi ile Bridge modelleri arasindaki compile-time eslemeleri uretir.
// sistemdeki gorevi: Mapper dosyasini yalniz saf Mapperly imzalarinda tutup semantik kurallari servise birakir.
[Mapper]
public partial class FailureDiagnosisMapper
{
    public partial ApiDiagnoseRequestDto Map(PtnApiDiagnosisRequest input);
    public partial DatabaseDiagnoseRequestDto Map(PtnDatabaseDiagnosisRequest input);
    public partial PtnDiagnosisReport Map(ApiDiagnosisReportDto input);
    public partial PtnDiagnosisReport Map(DatabaseDiagnosisReportDto input);
    public partial PtnApiFailureIdentity Map(ApiIdentityDto input);
    private partial ApiProblemErrorDto Map(PtnProblemError input);
    private partial DatabaseDiagnoseRequestDto.AssertionSignalDto Map(PtnDatabaseAssertionSignal input);
    private partial DatabaseDiagnoseRequestDto.DatabaseExceptionSignalDto Map(PtnDatabaseExceptionSignal input);
    private partial Ptn.DatabaseChecker.Dtos.Assertions.FailedExpectationDto Map(PtnFailedExpectation input);
    private partial PtnDiagnosisHypothesis Map(ApiHypothesisDto input);
    private partial PtnDiagnosisHypothesis Map(DatabaseHypothesisDto input);
    private partial PtnEvidence Map(ApiEvidenceDto input);
    private partial PtnEvidence Map(DatabaseEvidenceDto input);
}
