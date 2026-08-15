using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Runs;
using Ptn.TestModule.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Runs;

// islevi: Veritabaninda hesaplanmis senaryo saglik satirlarinin okunmasini orkestre eder.
// sistemdeki gorevi: Validation, salt-okunur repository ve Mapperly'yi baglar; hicbir oran uygulama tarafinda hesaplanmaz.
/// <summary>Senaryo saglik okuma use-case'lerinin Application uygulamasidir.</summary>
[RemoteService(IsEnabled = false)]
public class ScenarioHealthAppService : TestModuleAppService, IScenarioHealthAppService
{
    /// <summary>Saglik dikeyinin saf katmanlar-arasi eslemelerini yapar.</summary>
    private static readonly ScenarioHealthMapper Mapper = new();

    private readonly IScenarioHealthRepository _repository;
    private readonly ScenarioHealthReadManager _readManager;
    private readonly IValidator<ScenarioHealthListInput> _listValidator;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    public ScenarioHealthAppService(
        IScenarioHealthRepository repository,
        ScenarioHealthReadManager readManager,
        IValidator<ScenarioHealthListInput> listValidator,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _repository = repository;
        _readManager = readManager;
        _listValidator = listValidator;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    /// <summary>Senaryo saglik satirlarini filtreli ve kararli sayfalama ile getirir.</summary>
    public async Task<PagedResultDto<ScenarioHealthDto>> GetListAsync(ScenarioHealthListInput input)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        await _listValidator.ValidateAndThrowAsync(input, _cancellationTokenProvider.Token);
        var page = await _repository.GetPageAsync(Mapper.Map(input), _cancellationTokenProvider.Token);
        return new PagedResultDto<ScenarioHealthDto>(
            page.TotalCount,
            Mapper.Map(new List<ScenarioHealth>(page.Items)));
    }

    /// <summary>Tek senaryo anahtarinin saglik ozetini getirir.</summary>
    public async Task<ScenarioHealthDto> GetByScenarioKeyAsync(string scenarioKey)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var normalized = ScenarioHealthReadManager.NormalizeScenarioKey(scenarioKey);
        var row = await _repository.FindByScenarioKeyAsync(normalized, _cancellationTokenProvider.Token);
        return Mapper.Map(_readManager.EnsureFound(row, normalized));
    }
}
