using System.Threading.Tasks;
using FluentValidation;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Managers.Bridge.Profiles;
using Ptn.TestModule.Managers.Compilation;
using Ptn.TestModule.Mappers.Catalog;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Settings;
using Volo.Abp.Settings;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Catalog;

// islevi: Arazzo taslagini profil ile derleyip lint eder ve hicbir kaydi degistirmez.
// sistemdeki gorevi: Mevcut derleme Manager'ini public salt-hesap Application yuzeyine baglar.
public class ScenarioCompilationAppService : TestModuleAppService, IScenarioCompilationAppService
{
    private static readonly TestScenarioMapper Mapper = new();
    private readonly ArazzoCompilerManager _compilerManager;
    private readonly ProfilePackFileManager _profileFileManager;
    private readonly ProfilePackManager _profileManager;
    private readonly ISettingProvider _settingProvider;
    private readonly IValidator<ScenarioCompilePreviewDto> _validator;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    public ScenarioCompilationAppService(
        ArazzoCompilerManager compilerManager,
        ProfilePackFileManager profileFileManager,
        ProfilePackManager profileManager,
        ISettingProvider settingProvider,
        IValidator<ScenarioCompilePreviewDto> validator,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _compilerManager = compilerManager;
        _profileFileManager = profileFileManager;
        _profileManager = profileManager;
        _settingProvider = settingProvider;
        _validator = validator;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    public async Task<ScenarioCompilePreviewResultDto> CompilePreviewAsync(ScenarioCompilePreviewDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Update);
        var token = _cancellationTokenProvider.Token;
        await _validator.ValidateAndThrowAsync(input, token);
        var profileKey = await _settingProvider.GetOrNullAsync(TestModuleSettings.ProfilePackKey);
        var pack = await _profileFileManager.LoadAsync(profileKey!, token);
        var validated = _profileManager.GetValidatedForCompilation(pack, profileKey!, []);
        return Mapper.Map(await _compilerManager.CompileAsync(
            input.SourceDocument,
            validated,
            input.SpecSnapshotId,
            token));
    }
}
