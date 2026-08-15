using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Permissions;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Runs;

// islevi: Sirsiz ortam listesini ve ayri izinli sandbox resetini orkestre eder.
// sistemdeki gorevi: Setting manager ile sandbox capability arasindaki public Application yuzeyidir.
public class TestEnvironmentAppService : TestModuleAppService, ITestEnvironmentAppService
{
    private static readonly TestRunMapper Mapper = new();
    private readonly RunEnvironmentBindingManager _bindingManager;
    private readonly ITestDataSandbox _sandbox;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    public TestEnvironmentAppService(
        RunEnvironmentBindingManager bindingManager,
        ITestDataSandbox sandbox,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _bindingManager = bindingManager;
        _sandbox = sandbox;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    public async Task<List<TestEnvironmentBindingDto>> GetListAsync()
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.View);
        var bindings = await _bindingManager.ListAsync(_cancellationTokenProvider.Token);
        return Mapper.Map(new List<TestRunEnvironmentBinding>(bindings));
    }

    public async Task ResetSandboxAsync(string key)
    {
        await CheckPolicyAsync(TestModulePermissions.Runs.SandboxReset);
        await _bindingManager.ResolveAsync(key, _cancellationTokenProvider.Token);
        await _sandbox.ResetAsync(key, _cancellationTokenProvider.Token);
    }
}
