using System;
using System.Linq;
using System.Reflection;
using Ptn.TestModule.BackgroundWorkers.Catalog;
using Ptn.TestModule.BackgroundWorkers.Runs;
using Ptn.TestModule.EventHandlers.Runs;
using Shouldly;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EventBus;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Modulun periyodik tarayicilarinin ve olay abonesinin kompozisyon kokune bagli kaldigini dogrular.
// sistemdeki gorevi: Yazilmis ama hicbir zaman calismayan worker regresyonunu derlenmeden yakalar (KBP-110 Dilim 6).
public class BackgroundWorkerRegistrationTests
{
    private const string RegistrationMethodName = "OnApplicationInitializationAsync";

    // Uc tarayici da ABP worker tabanindan turemeli ve tik davranisini kendi DoWorkAsync'inde tanimlamalidir.
    [Theory]
    [InlineData(typeof(ExpiredQuarantineSweepWorker))]
    [InlineData(typeof(ScenarioHealthRefreshWorker))]
    [InlineData(typeof(DueScenarioRunWorker))]
    public void Every_periodic_worker_should_derive_from_the_abp_worker_base_and_declare_a_tick(Type workerType)
    {
        typeof(AsyncPeriodicBackgroundWorkerBase).IsAssignableFrom(workerType).ShouldBeTrue();
        workerType.GetMethod(
                "DoWorkAsync",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ShouldNotBeNull();
    }

    // Her tarayici kompozisyon kokunde acikca kaydedilmelidir; kayitsiz worker hicbir zaman tiklamaz.
    [Theory]
    [InlineData(nameof(ExpiredQuarantineSweepWorker))]
    [InlineData(nameof(ScenarioHealthRefreshWorker))]
    [InlineData(nameof(DueScenarioRunWorker))]
    public void Every_periodic_worker_should_be_registered_in_the_application_module(string workerName)
    {
        var registration = typeof(TestModuleApplicationModule).GetMethod(
            RegistrationMethodName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        registration.ShouldNotBeNull();
        ReadModuleSource().ShouldContain($"AddBackgroundWorkerAsync<{workerName}>");
    }

    // Sozlesme degisikligi abonesi checker olayini local event bus uzerinden dinlemelidir.
    [Fact]
    public void Contract_change_handler_should_listen_to_the_checker_status_event()
    {
        typeof(ContractChangeTriggerHandler).GetInterfaces()
            .Any(contract => contract.IsGenericType &&
                             contract.GetGenericTypeDefinition() == typeof(ILocalEventHandler<>))
            .ShouldBeTrue();
    }

    // Kompozisyon kokunun kaynagini okuyup kayit satirlarini metin olarak dogrular.
    private static string ReadModuleSource()
    {
        return TestModuleSourceReader.Read(
            "src",
            "Ptn.TestModule.Application",
            "TestModuleApplicationModule.cs");
    }
}
