using System;
using System.IO;
using CheckNexus.Vault;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Testing;
using Xunit;
using ApiSecretProvider = Ptn.ApiContractChecker.Interface.Secrets.ISecretProvider;
using DatabaseSecretProvider = Ptn.DatabaseChecker.Interface.Secrets.ISecretProvider;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Test Module hostunun ortak Vault adapter kompozisyonunu dogrular.
// sistemdeki gorevi: Iki checker secret portunun ayni singleton provider'a bagli kalmasini guvenceye alir.
public class VaultCompositionTests : AbpIntegratedTest<VaultCompositionTestModule>
{
    // Host module graph'ini ve iki secret portunun ortak singleton kaydini dogrular.
    [Fact]
    public void Host_should_compose_one_vault_provider_for_both_checker_ports()
    {
        var hostModuleSource = File.ReadAllText(Path.Combine(
            FindModuleRoot().FullName,
            "host",
            "Ptn.TestModule.HttpApi.Host",
            "TestModuleHttpApiHostModule.cs"));
        var apiProvider = ServiceProvider.GetRequiredService<ApiSecretProvider>();
        var databaseProvider = ServiceProvider.GetRequiredService<DatabaseSecretProvider>();

        hostModuleSource.ShouldContain("typeof(CheckNexusVaultModule)");
        apiProvider.ShouldBeSameAs(databaseProvider);
        apiProvider.ShouldBeOfType<VaultSecretProvider>();
    }

    // Test assembly konumundan Test Module cozum kokunu bulur.
    private static DirectoryInfo FindModuleRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ptn.TestModule.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new DirectoryNotFoundException("Ptn.TestModule.slnx bulunamadi.");
    }
}
