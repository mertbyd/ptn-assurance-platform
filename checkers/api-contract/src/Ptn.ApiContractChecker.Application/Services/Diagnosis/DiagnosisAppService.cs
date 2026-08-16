using FluentValidation;
using Ptn.ApiContractChecker.Application.Mappers.Diagnosis;
using Ptn.ApiContractChecker.Dtos.Diagnosis;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Diagnosis;
using Ptn.ApiContractChecker.Managers.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Services.Diagnosis;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Ptn.ApiContractChecker.Diagnostics;
using Ptn.ApiContractChecker.Constants.Diagnostics;

namespace Ptn.ApiContractChecker.Application.Services.Diagnosis;

// islevi: Diagnosis girdisini dogrular, snapshot/findings yukler, Mapperly ile tasir ve manager'i cagirir.
// sistemdeki gorevi: HTTP siniri ile yedi adimli domain teshis motoru arasinda ince application orkestrasyonu saglar.
[RemoteService(IsEnabled = false)]
public class DiagnosisAppService : ApiContractCheckerAppService, IDiagnosisAppService
{
    private static readonly DiagnosisMapper Mapper = new();
    private ISpecSnapshotRepository SnapshotRepository => LazyGetRequiredService<ISpecSnapshotRepository>();
    private ContractCheckRunManager RunManager => LazyGetRequiredService<ContractCheckRunManager>();
    private DiagnosisManager Manager => LazyGetRequiredService<DiagnosisManager>();
    private IValidator<DiagnoseRequestDto> Validator => LazyGetRequiredService<IValidator<DiagnoseRequestDto>>();

    public DiagnosisAppService(IAbpLazyServiceProvider provider) : base(provider)
    {
    }

    // islevi: Tek basarisiz adim sinyalini kalici olmayan ve butceli RFC 9457 raporuna cevirir.
    public async Task<DiagnosisReportDto> DiagnoseAsync(DiagnoseRequestDto input)
    {
        using var activity = ApiContractCheckerActivity.Start(
            ApiContractCheckerDiagnostics.DiagnosisRunSpan,
            ApiContractCheckerDiagnostics.MomentDiagnosis,
            input.ContractCheckRunId);
        await Validator.ValidateAndThrowAsync(input);
        var snapshot = await SnapshotRepository.FindWithDetailsAsync(input.SnapshotId);
        var signal = Mapper.MapToSignal(input);
        var findings = new List<Finding>();
        if (input.ContractCheckRunId.HasValue)
        {
            findings = (await RunManager.GetRequiredDetailAsync(input.ContractCheckRunId.Value)).Findings.Items;
        }
        var report = await Manager.DiagnoseAsync(snapshot, signal, findings);
        var result = Mapper.MapToDto(report);
        ApiContractCheckerActivity.SetResponseBytes(activity, report.MeasureUtf8Bytes());
        return result;
    }
}
