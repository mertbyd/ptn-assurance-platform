using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Querying;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Interface.Lookups;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Runs;
using Volo.Abp.Domain.Entities;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Sozlesme degisikligi olayinin islem gerektirip gerektirmedigine karar verir ve etkilenen senaryolari cozer.
// sistemdeki gorevi: Eslesme snapshot seviyesindedir; operasyon seviyesi eslesme TM-22b'dir ve olculene kadar acilmaz.
/// <summary>
/// API sozlesmesi degisikliginin test tarafindaki etkisini kaba ama acik bir kuralla belirler.
/// </summary>
public class ContractChangeImpactManager : TestModuleDomainService
{
    private readonly ITestScenarioRepository _scenarioRepository;
    private readonly ITestScenarioStateRepository _stateRepository;

    public ContractChangeImpactManager(
        ITestScenarioRepository scenarioRepository,
        ITestScenarioStateRepository stateRepository)
    {
        _scenarioRepository = scenarioRepository;
        _stateRepository = stateRepository;
    }

    // Yalniz tamamlanmis, yeni bulgu tasiyan ve en agir siddeti breaking olan kosular is uretir.
    /// <summary>Olayin kosum tetiklemeye deger olup olmadigini bildirir.</summary>
    public static bool IsActionable(ContractChangeSignal signal)
    {
        ArgumentNullException.ThrowIfNull(signal);
        return string.Equals(signal.StatusCode, CheckRunStatusCodes.Completed, StringComparison.Ordinal) &&
               signal.NewFindingCount > 0 &&
               string.Equals(signal.MaxSeverityCode, DifferenceSeverityCodes.Breaking, StringComparison.Ordinal);
    }

    // Etkilenen senaryolar eski sozlesmeye muhurlenmis yayinlanmis surumlerdir; karantinadakiler disaridadir.
    /// <summary>Base snapshot'a muhurlenmis yayinlanmis senaryolari getirir.</summary>
    public async Task<IReadOnlyList<DueScenarioModel>> GetAffectedScenariosAsync(
        Guid baseSnapshotId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var publishedState = await _stateRepository.FindAsync(
            new RepositoryQuery<TestScenarioState>()
                .Where(item => item.Code == TestScenarioStateCodes.Published),
            cancellationToken);
        if (publishedState is null)
        {
            throw new EntityNotFoundException(typeof(TestScenarioState));
        }

        return await _scenarioRepository.GetPublishedBySpecSnapshotAsync(
            baseSnapshotId,
            publishedState.Id,
            now,
            TestModuleCatalogSettingNames.MaxContractChangeScenariosPerEvent,
            cancellationToken);
    }

    // Ayni kontrol kosusu ile ayni senaryo cifti icin ikinci kosum uretilmemesini saglayan referansi kurar.
    /// <summary>Kontrol kosusu kimliginden kararli tetikleyici referansi uretir.</summary>
    public static string CreateTriggerRef(Guid checkRunId) => checkRunId.ToString("D");
}
