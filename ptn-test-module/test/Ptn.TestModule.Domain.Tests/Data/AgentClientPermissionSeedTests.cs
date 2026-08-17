using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Ptn.TestModule.Data;
using Ptn.TestModule.Permissions;
using Shouldly;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.PermissionManagement;
using Xunit;

namespace Ptn.TestModule.Domain.Tests.Data;

// islevi: Makine istemcisi izin seed'inin saglayici, anahtar ve izin adi kurallarini dogrular.
// sistemdeki gorevi: client-credentials token'i kullanici tasimadigi icin yetki "C" saglayicisindan okunur;
// yanlis saglayici veya yazim hatasi sessiz 403 uretir, bu testler onu derlemeden once yakalar.
public class AgentClientPermissionSeedTests
{
    private const string AgentClientId = "TestModule_Agent";

    // Tanimli izinler ABP'nin client saglayicisina istemci kimligiyle yazilmalidir.
    [Fact]
    public async Task Should_grant_configured_permissions_to_the_client_provider()
    {
        var permissionDataSeeder = Substitute.For<IPermissionDataSeeder>();
        var contributor = NewContributor(permissionDataSeeder, TestModulePermissions.Bridge.Ground);

        await contributor.SeedAsync(new DataSeedContext());

        await permissionDataSeeder.Received(1).SeedAsync(
            ClientPermissionValueProvider.ProviderName,
            AgentClientId,
            Arg.Is<IEnumerable<string>>(names => names.Contains(TestModulePermissions.Bridge.Ground)),
            Arg.Any<Guid?>());
    }

    // Bu modulde tanimli olmayan izin adi grant uretmeden reddedilmelidir.
    [Fact]
    public async Task Should_reject_a_permission_that_this_module_does_not_define()
    {
        var permissionDataSeeder = Substitute.For<IPermissionDataSeeder>();
        var contributor = NewContributor(permissionDataSeeder, "AbpIdentity.Users.Delete");

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => contributor.SeedAsync(new DataSeedContext()));

        exception.Message.ShouldContain("AbpIdentity.Users.Delete");
        exception.Message.ShouldContain(AgentClientId);
        await permissionDataSeeder.DidNotReceiveWithAnyArgs().SeedAsync(default!, default!, default!, default);
    }

    // Katkiciyi tek istemci kaydi tasiyan bellek ici yapilandirmayla kurar.
    private static AgentClientPermissionDataSeedContributor NewContributor(
        IPermissionDataSeeder permissionDataSeeder,
        string permissionName)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AgentClients:Registrations:0:ClientId"] = AgentClientId,
                ["AgentClients:Registrations:0:Permissions:0"] = permissionName
            })
            .Build();

        return new AgentClientPermissionDataSeedContributor(configuration, permissionDataSeeder);
    }
}
