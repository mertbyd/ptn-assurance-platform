using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Dtos.Diagnosis;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Diagnosis;

// islevi: Diagnose request/signal ve domain report/DTO katman donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: AppService ve manager'da elle property kopyalama olmadan public sozlesmeyi domain cekirdegine baglar.
[Mapper]
public partial class DiagnosisMapper
{
    // islevi: Public diagnose request'i persist edilmeyen domain failure sinyaline tasir.
    public partial FailureSignal MapToSignal(DiagnoseRequestDto input);

    // islevi: Nested assertion sinyal DTO'sunu domain assertion sinyaline tasir.
    public partial FailureSignal.AssertionFailureSignal MapAssertionSignal(
        DiagnoseRequestDto.AssertionSignalDto input);

    // islevi: Nested database-exception DTO'sunu yapilandirilmis domain sinyaline tasir.
    public partial FailureSignal.DatabaseExceptionFailureSignal MapDatabaseExceptionSignal(
        DiagnoseRequestDto.DatabaseExceptionSignalDto input);

    // islevi: Failed-expectation DTO'sunu teshis domain kanit girdisine tasir.
    public partial FailedExpectation MapFailedExpectation(FailedExpectationDto input);

    // islevi: Domain RFC raporunu public diagnosis DTO'suna tasir.
    public partial DiagnosisReportDto MapToDto(DiagnosisReport input);

    // islevi: Domain identity modelini nested public identity DTO'suna tasir.
    public partial DiagnosisReportDto.IdentityDto MapIdentity(FailureIdentity input);

    // islevi: Dogrulanmis domain object reference'i nested public location DTO'suna tasir.
    public partial DiagnosisReportDto.LocationDto MapLocation(ObjectReference input);

    // islevi: Domain hipotez sonucunu nested public hipotez DTO'suna tasir.
    public partial DiagnosisReportDto.HypothesisDto MapHypothesis(HypothesisAssessment input);

    // islevi: Domain probe kanitini nested public evidence DTO'suna tasir.
    public partial DiagnosisReportDto.EvidenceDto MapEvidence(ProbeEvidence input);
}
