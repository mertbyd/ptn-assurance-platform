using Ptn.ApiContractChecker.Dtos.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Riok.Mapperly.Abstractions;

namespace Ptn.ApiContractChecker.Application.Mappers.Diagnosis;

// islevi: Diagnosis request/response DTO'lari ile domain signal ve RFC report modellerini Mapperly ile tasir.
// sistemdeki gorevi: Application katmanindaki butun diagnosis alan kopyalamanin tek compile-time sahibidir.
[Mapper]
public partial class DiagnosisMapper
{
    public partial HttpFailureSignal MapToSignal(DiagnoseRequestDto input);
    public partial ProblemErrorSignal MapToModel(ProblemErrorDto input);
    public partial DiagnosisReportDto MapToDto(DiagnosisReport report);
    public partial ProblemErrorDto MapToDto(ProblemErrorSignal model);
    public partial FailureIdentityDto MapToDto(FailureIdentity model);
    public partial ObjectReferenceDto MapToDto(ObjectReference model);
    public partial ProbeEvidenceDto MapToDto(ProbeEvidence model);
    public partial SuggestedCheckDto MapToDto(SuggestedCheck model);
    public partial HypothesisAssessmentDto MapToDto(HypothesisAssessment model);
}
