using System;
using System.Linq;
using Ptn.TestModule.Services.Runs;
using Shouldly;
using Volo.Abp.Application.Services;
using Volo.Abp.Modularity;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Application.Contracts sozlesmelerinin AddHttpClientProxies tarafindan proxy'lenebilir kaldigini tarar.
// sistemdeki gorevi: Istemci yuzeyinden sessizce dusen sozlesme regresyonunu derlenmeden yakalar.
public class ClientProxySurfaceTests
{
    // Sozlesme arayuzlerinin namespace koku; yalniz bu agac istemciye tasinir.
    private const string ServiceNamespaceRoot = "Ptn.TestModule.Services";

    // AddHttpClientProxies yalniz IApplicationService turevlerine proxy uretir; turemeyen sozlesme istemciden sessizce duser.
    [Fact]
    public void Every_app_service_contract_should_be_client_proxy_eligible()
    {
        var ineligible = GetContractTypes()
            .Where(type => !typeof(IApplicationService).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        ineligible.ShouldBeEmpty();
    }

    // Istemci modulu sozlesme assembly'sini taramazsa uretilen proxy kumesi bosalir.
    [Fact]
    public void Client_module_should_scan_the_application_contracts_assembly()
    {
        var scanned = typeof(TestModuleHttpApiClientModule)
            .GetCustomAttributes(typeof(DependsOnAttribute), false)
            .Cast<DependsOnAttribute>()
            .SelectMany(attribute => attribute.GetDependedTypes())
            .ToArray();

        scanned.ShouldContain(typeof(TestModuleApplicationContractsModule));
    }

    // Uzak servis adi controller rotalari ve yayimlanmis istemciler arasinda paylasilan tek sabittir.
    [Fact]
    public void Remote_service_name_should_stay_stable()
    {
        TestModuleRemoteServiceConsts.RemoteServiceName.ShouldBe("TestModule");
    }

    // Olcumun bos kumede kazara yesil kalmasini engeller; sozlesme grafigi daralirsa test uyarir.
    [Fact]
    public void Client_proxy_surface_should_stay_populated()
    {
        GetContractTypes().Length.ShouldBeGreaterThan(10);
    }

    // Application.Contracts icindeki public sozlesme arayuzlerini toplar.
    private static Type[] GetContractTypes()
    {
        return typeof(ITestRunAppService).Assembly
            .GetTypes()
            .Where(type => type.IsInterface && type.IsPublic)
            .Where(type => type.Namespace is not null &&
                           type.Namespace.StartsWith(ServiceNamespaceRoot, StringComparison.Ordinal))
            .ToArray();
    }
}
