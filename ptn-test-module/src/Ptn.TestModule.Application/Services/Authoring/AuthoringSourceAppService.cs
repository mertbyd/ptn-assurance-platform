using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Managers.Bridge.Profiles;
using Ptn.TestModule.Mappers.Authoring;
using Ptn.TestModule.Permissions;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Authoring;

// islevi: Is kurali ve profil paketi malzemesinin yuklenmesini, dogrulanmasini ve listelenmesini orkestre eder.
// sistemdeki gorevi: MCP Resource'un okudugu bayt ile muhurlenen baytin ayni dosyadan gelmesini saglayan use-case yuzeyidir.
public class AuthoringSourceAppService : TestModuleAppService, IAuthoringSourceAppService
{
    private static readonly AuthoringSourceMapper Mapper = new();
    private readonly IBusinessRuleSourcePort _businessRuleSource;
    private readonly IProfilePackSourcePort _profilePackSource;
    private readonly BusinessRuleFingerprintManager _fingerprintManager;
    private readonly ProfilePackFileManager _profilePackFileManager;
    private readonly ProfilePackManager _profilePackManager;
    private readonly IValidator<UploadBusinessRulesDto> _businessRulesValidator;
    private readonly IValidator<UploadProfilePackDto> _profilePackValidator;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    // Yukleme use-case'lerini mevcut kaynak portlarina ve muhur sahiplerine baglar.
    public AuthoringSourceAppService(
        IBusinessRuleSourcePort businessRuleSource,
        IProfilePackSourcePort profilePackSource,
        BusinessRuleFingerprintManager fingerprintManager,
        ProfilePackFileManager profilePackFileManager,
        ProfilePackManager profilePackManager,
        IValidator<UploadBusinessRulesDto> businessRulesValidator,
        IValidator<UploadProfilePackDto> profilePackValidator,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _businessRuleSource = businessRuleSource;
        _profilePackSource = profilePackSource;
        _fingerprintManager = fingerprintManager;
        _profilePackFileManager = profilePackFileManager;
        _profilePackManager = profilePackManager;
        _businessRulesValidator = businessRulesValidator;
        _profilePackValidator = profilePackValidator;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    // Is kurali metnini sabit adli tek kaynaga yazar ve yazilan baytlarin muhrunu dondurur.
    public async Task<AuthoringSourceDto> UploadBusinessRulesAsync(UploadBusinessRulesDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Bridge.ManageSources);
        var token = _cancellationTokenProvider.Token;
        await _businessRulesValidator.ValidateAndThrowAsync(input, token);
        var content = Encoding.UTF8.GetBytes(input.Content);
        await _businessRuleSource.WriteAsync(content, token);
        return Mapper.Map(_fingerprintManager.Seal(PtnBridgeSettingNames.BusinessRulesFileName, content));
    }

    // Profil paketini yazar ve ayni anahtardan geri okuyarak paketin cozumlenebilirligini kanitlar.
    public async Task<AuthoringSourceDto> UploadProfilePackAsync(UploadProfilePackDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Bridge.ManageSources);
        var token = _cancellationTokenProvider.Token;
        await _profilePackValidator.ValidateAndThrowAsync(input, token);
        var content = Encoding.UTF8.GetBytes(input.Content);
        await _profilePackSource.WriteAsync(input.ProfileKey, content, token);
        var stored = await _profilePackFileManager.LoadAsync(input.ProfileKey, token);
        return Mapper.Map(_profilePackManager.Seal(
            stored,
            input.ProfileKey + PtnBridgeSettingNames.ProfilePackExtension,
            content.Length));
    }

    // Yuklu paketleri kokten okur ve her birinin kapsam sayilarini ozet olarak dondurur.
    public async Task<List<ProfilePackSummaryDto>> GetProfilePacksAsync()
    {
        await CheckPolicyAsync(TestModulePermissions.Bridge.ManageSources);
        var token = _cancellationTokenProvider.Token;
        var keys = await _profilePackSource.ListKeysAsync(token);
        var summaries = new List<ProfilePackSummaryDto>();
        foreach (var key in keys)
        {
            var pack = await _profilePackFileManager.LoadAsync(key, token);
            summaries.Add(Mapper.Map(_profilePackManager.Summarize(pack)));
        }

        return summaries;
    }
}
