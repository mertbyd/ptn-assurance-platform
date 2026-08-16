using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.EntityFrameworkCore;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;

namespace Ptn.TestModule.EntityFrameworkCore.Tests.Composition;

// islevi: AddAlwaysAllowAuthorization() etkisini kaldiran izole test modulu.
// sistemdeki gorevi: Yetki denetimi kapilarinin davranissal olarak test edilmesini saglar.
[DependsOn(
    typeof(TestModuleEntityFrameworkCoreTestModule)
)]
public class TestModuleIsolatedAuthTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // KBP-117 Test DI kısıtı: Prod tarafında [ExposeServices] eksiği olan test bağımlılıklarını burada karşıla
        context.Services.AddTransient<Ptn.TestModule.Interface.Authoring.IAuthoringSessionStore, Ptn.TestModule.Services.Authoring.AuthoringSessionCacheService>();
        context.Services.AddTransient<Ptn.TestModule.Interface.Compilation.IScenarioCompilationPort, Ptn.TestModule.Services.Compilation.ScenarioCompilationService>();
        context.Services.AddTransient<Ptn.TestModule.Interface.Shared.IProcessBoundaryPort, Ptn.TestModule.Services.Shared.ProcessBoundaryService>();

        // AddAlwaysAllowAuthorization tarafindan eklenen sahte yetkilendiriciyi kaldir, gercek yetki denetimi calissin.
        var alwaysAllowDescriptor = context.Services.FirstOrDefault(s => s.ImplementationType != null && s.ImplementationType.Name == "AlwaysAllowAuthorizationService");
        if (alwaysAllowDescriptor != null)
        {
            context.Services.Remove(alwaysAllowDescriptor);
            // AlwaysAllow kaldirildiginda IAbpAuthorizationService bos kalir; gercek Abp yetkilendirmeyi geri yukle.
            context.Services.AddTransient<Microsoft.AspNetCore.Authorization.IAuthorizationService, Volo.Abp.Authorization.AbpAuthorizationService>();
            context.Services.AddTransient<Volo.Abp.Authorization.IAbpAuthorizationService, Volo.Abp.Authorization.AbpAuthorizationService>();
        }
        
        // HostEnvironment genelde testlerde yoktur ama ProfilePackFileManager'in buna ihtiyaci var.
        context.Services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(new FakeHostEnvironment());
        
        // DatabaseCheckerContracts ve ApiContractCheckerContracts ile gelen ama uygulamasi olmayan dis servisleri mockla.
        context.Services.AddSingleton(NSubstitute.Substitute.For<Ptn.DatabaseChecker.Services.SchemaDiscovery.ISchemaDiscoveryAppService>());
        context.Services.AddSingleton(NSubstitute.Substitute.For<Ptn.DatabaseChecker.Services.Capabilities.IWriteSetCapabilityAppService>());
        context.Services.AddSingleton(NSubstitute.Substitute.For<Ptn.DatabaseChecker.Services.Assertions.IDatabaseAssertionAppService>());
        context.Services.AddSingleton(NSubstitute.Substitute.For<Ptn.DatabaseChecker.Services.Projections.IProjectionAppService>());
        context.Services.AddSingleton(NSubstitute.Substitute.For<Ptn.DatabaseChecker.Services.Assertions.IAssertionDerivabilityAppService>());
        context.Services.AddSingleton(NSubstitute.Substitute.For<Ptn.DatabaseChecker.Services.Diagnosis.IDiagnosisAppService>());
        context.Services.AddSingleton(NSubstitute.Substitute.For<Ptn.ApiContractChecker.Services.Conformance.IResponseConformanceAppService>());
        context.Services.AddSingleton(NSubstitute.Substitute.For<Ptn.ApiContractChecker.Services.Snapshots.ISpecSnapshotAppService>());
        context.Services.AddSingleton(NSubstitute.Substitute.For<Ptn.ApiContractChecker.Services.Diagnosis.IDiagnosisAppService>());
        
        // Sahte ama davranissal bir IPermissionChecker kaydet.
        context.Services.AddTransient<IPermissionChecker, FakePermissionChecker>();
    }
}

public class FakeHostEnvironment : Microsoft.Extensions.Hosting.IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "Ptn.TestModule";
    public string ContentRootPath { get; set; } = System.IO.Directory.GetCurrentDirectory();
    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
}

public class FakePermissionChecker : IPermissionChecker, ITransientDependency
{
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public FakePermissionChecker(ICurrentPrincipalAccessor currentPrincipalAccessor)
    {
        _currentPrincipalAccessor = currentPrincipalAccessor;
    }

    public Task<bool> IsGrantedAsync(string name)
    {
        return IsGrantedAsync(_currentPrincipalAccessor.Principal, name);
    }

    public Task<bool> IsGrantedAsync(ClaimsPrincipal? claimsPrincipal, string name)
    {
        var isGranted = claimsPrincipal?.HasClaim("TestPermission", name) ?? false;
        return Task.FromResult(isGranted);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(string[] names)
    {
        return IsGrantedAsync(_currentPrincipalAccessor.Principal, names);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(ClaimsPrincipal? claimsPrincipal, string[] names)
    {
        var result = new MultiplePermissionGrantResult();
        foreach (var name in names)
        {
            result.Result.Add(name, claimsPrincipal?.HasClaim("TestPermission", name) == true ? PermissionGrantResult.Granted : PermissionGrantResult.Undefined);
        }
        return Task.FromResult(result);
    }
}
