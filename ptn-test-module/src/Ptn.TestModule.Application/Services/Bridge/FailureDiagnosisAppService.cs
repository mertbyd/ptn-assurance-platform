using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Volo.Abp;
using ApiDiagnosisService = Ptn.ApiContractChecker.Services.Diagnosis.IDiagnosisAppService;
using DatabaseDiagnosisService = Ptn.DatabaseChecker.Services.Diagnosis.IDiagnosisAppService;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Iki checker diagnosis servisini ortak Bridge use-case'lerinde birlestirir.
// sistemdeki gorevi: Checker cagrisi ve Mapperly orkestrasyonunu yapip tum kararlari Manager'a birakir.
[RemoteService(IsEnabled = false)]
public class FailureDiagnosisAppService : TestModuleAppService, IFailureDiagnosisAppService
{
    private static readonly FailureDiagnosisMapper Mapper = new();
    private readonly ApiDiagnosisService _apiDiagnosisService;
    private readonly DatabaseDiagnosisService _databaseDiagnosisService;
    private readonly FailureDiagnosisManager _manager;
    private readonly IValidator<DiagnosisRequestDto> _validator;

    // Iki checker public diagnosis servisini ve ortak kural sahibini baglar.
    public FailureDiagnosisAppService(
        ApiDiagnosisService apiDiagnosisService,
        DatabaseDiagnosisService databaseDiagnosisService,
        FailureDiagnosisManager manager,
        IValidator<DiagnosisRequestDto> validator)
    {
        _apiDiagnosisService = apiDiagnosisService;
        _databaseDiagnosisService = databaseDiagnosisService;
        _manager = manager;
        _validator = validator;
    }

    // Public API diagnosis girdisini Domain modeline ve normalize sonucu DTO'ya map eder.
    public async Task<DiagnosisReportDto> DiagnoseApiAsync(
        DiagnosisRequestDto input,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(
            input,
            options =>
            {
                options.ThrowOnFailures();
                options.IncludeRulesNotInRuleSet();
                options.IncludeRuleSets(PtnBridgeValidationRuleSets.Api);
            },
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var request = Mapper.Map(input);
        var source = await _apiDiagnosisService.DiagnoseAsync(Mapper.Map(_manager.CreateApiRequest(request)));
        return Mapper.Map(_manager.NormalizeApiReport(Mapper.Map(source)));
    }

    // Public database diagnosis girdisini Domain modeline ve normalize sonucu DTO'ya map eder.
    public async Task<DiagnosisReportDto> DiagnoseDatabaseAsync(
        DiagnosisRequestDto input,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(
            input,
            options =>
            {
                options.ThrowOnFailures();
                options.IncludeRulesNotInRuleSet();
                options.IncludeRuleSets(PtnBridgeValidationRuleSets.Database);
            },
            cancellationToken);
        var request = Mapper.Map(input);
        var source = await _databaseDiagnosisService.DiagnoseAsync(
            Mapper.Map(_manager.CreateDatabaseRequest(request)),
            cancellationToken);
        return Mapper.Map(_manager.NormalizeDatabaseReport(Mapper.Map(source)));
    }
}
