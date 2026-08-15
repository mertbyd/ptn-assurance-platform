using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Mappers.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Runs;

// islevi: Filtreli bulgu sayfasini validation, repository ve Mapperly ile orkestre eder.
// sistemdeki gorevi: Bulgu query endpoint'inin duz Application use-case'idir.
public class TestFindingAppService : TestModuleAppService, ITestFindingAppService
{
    private static readonly TestRunMapper Mapper = new();
    private readonly ITestRunRepository _repository;
    private readonly IValidator<TestFindingListInput> _validator;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    public TestFindingAppService(
        ITestRunRepository repository,
        IValidator<TestFindingListInput> validator,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _repository = repository;
        _validator = validator;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    public async Task<PagedResultDto<TestFindingHeaderDto>> GetListAsync(TestFindingListInput input)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        await _validator.ValidateAndThrowAsync(input, _cancellationTokenProvider.Token);
        var page = await _repository.GetFindingPageAsync(Mapper.Map(input), _cancellationTokenProvider.Token);
        return new PagedResultDto<TestFindingHeaderDto>(page.TotalCount, Mapper.Map(new List<TestFindingHeader>(page.Items)));
    }
}
