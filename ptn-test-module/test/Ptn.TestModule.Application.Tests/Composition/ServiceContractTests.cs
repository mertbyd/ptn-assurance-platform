using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Services.Catalog;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Application/Services altindaki her public servisin sozlesme arayuzu veya Domain port'u tasidigini dogrular.
// sistemdeki gorevi: Tuketilemeyen ve degistirilemeyen sozlesmesiz servisleri derlenmeden yakalar (house profile: contracts live in Application.Contracts).
public class ServiceContractTests
{
    // Application servis namespace'inin koku; yalniz bu agac altindaki tipler sozlesme tasimak zorundadir.
    private const string ServiceNamespaceRoot = "Ptn.TestModule.Services";

    // Application.Contracts icindeki sozlesme arayuzlerinin namespace koku.
    private const string ContractNamespaceRoot = "Ptn.TestModule.Services";

    // Domain port arayuzlerinin namespace koku.
    private const string PortNamespaceRoot = "Ptn.TestModule.Interface";

    // Her servis ya Application.Contracts sozlesmesini ya da Domain port'unu uygulamalidir.
    [Fact]
    public void Every_application_service_should_expose_a_contract_or_a_domain_port()
    {
        var contractless = GetServiceTypes()
            .Where(service => !HasContractOrPort(service))
            .Select(service => service.FullName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        contractless.ShouldBeEmpty();
    }

    // Olcumun bos kumede kazara yesil kalmasini engeller; servis grafigi daralirsa test uyarir.
    [Fact]
    public void Service_surface_should_stay_populated()
    {
        GetServiceTypes().Count.ShouldBeGreaterThan(10);
    }

    // Application assembly'sindeki somut servis tiplerini toplar.
    private static IReadOnlyList<Type> GetServiceTypes()
    {
        return typeof(TestScenarioAppService).Assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsPublic)
            .Where(type => type.Namespace is not null &&
                           type.Namespace.StartsWith(ServiceNamespaceRoot, StringComparison.Ordinal))
            .ToList();
    }

    // Servisin dogrudan uyguladigi arayuzler arasinda sozlesme veya port arar.
    private static bool HasContractOrPort(Type service)
    {
        return service.GetInterfaces().Any(IsContractOrPort);
    }

    // Sozlesme arayuzu Application.Contracts'ta, port ise Domain assembly'sinde yasar.
    private static bool IsContractOrPort(Type candidate)
    {
        if (candidate.Namespace is null)
        {
            return false;
        }

        if (candidate.Assembly == typeof(TestScenarioManager).Assembly)
        {
            return candidate.Namespace.StartsWith(PortNamespaceRoot, StringComparison.Ordinal);
        }

        return candidate.Assembly == typeof(ITestScenarioAppService).Assembly &&
               candidate.Namespace.StartsWith(ContractNamespaceRoot, StringComparison.Ordinal);
    }
}
