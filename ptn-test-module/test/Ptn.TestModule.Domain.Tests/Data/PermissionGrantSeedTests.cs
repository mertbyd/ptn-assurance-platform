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

// islevi: Izin grant seed'inin saglayici, anahtar ve izin adi kurallarini dogrular.
// sistemdeki gorevi: Makine istemcisi client_id, insan kullanicisi rolu uzerinden yetkilenir; yanlis saglayici
// veya yazim hatasi sessiz 403 uretir, bu testler onu derlemeden once yakalar.
public class PermissionGrantSeedTests
{
    private const string AgentClientId = "TestModule_Agent";
    private const string AdminRoleName = "SuperAdmin";

    // Makine istemcisi izinleri ABP'nin client saglayicisina istemci kimligiyle yazilmalidir.
    [Fact]
    public async Task Should_grant_configured_permissions_to_the_client_provider()
    {
        var permissionDataSeeder = Substitute.For<IPermissionDataSeeder>();
        var contributor = NewContributor(
            permissionDataSeeder,
            out var permissionDefinitionManager,
            agentPermission: TestModulePermissions.Bridge.Ground);

        await contributor.SeedAsync(new DataSeedContext());

        await permissionDataSeeder.Received(1).SeedAsync(
            ClientPermissionValueProvider.ProviderName,
            AgentClientId,
            Arg.Is<IEnumerable<string>>(names => names.Contains(TestModulePermissions.Bridge.Ground)),
            Arg.Any<Guid?>());
        await permissionDefinitionManager.Received(1).GetPermissionsAsync();
    }

    // Yonetici rolu izinleri ABP'nin rol saglayicisina rol adiyla yazilmalidir.
    [Fact]
    public async Task Should_grant_configured_permissions_to_the_role_provider()
    {
        var permissionDataSeeder = Substitute.For<IPermissionDataSeeder>();
        var contributor = NewContributor(
            permissionDataSeeder,
            out _,
            rolePermission: TestModulePermissions.Scenarios.Create);

        await contributor.SeedAsync(new DataSeedContext());

        await permissionDataSeeder.Received(1).SeedAsync(
            RolePermissionValueProvider.ProviderName,
            AdminRoleName,
            Arg.Is<IEnumerable<string>>(names => names.Contains(TestModulePermissions.Scenarios.Create)),
            Arg.Any<Guid?>());
    }

    // Bu modulde tanimli olmayan izin adi hicbir grant uretmeden reddedilmelidir.
    [Fact]
    public async Task Should_reject_a_permission_that_this_module_does_not_define()
    {
        var permissionDataSeeder = Substitute.For<IPermissionDataSeeder>();
        var contributor = NewContributor(
            permissionDataSeeder,
            out _,
            agentPermission: "AbpIdentity.Users.Delete");

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => contributor.SeedAsync(new DataSeedContext()));

        exception.Message.ShouldContain("AbpIdentity.Users.Delete");
        exception.Message.ShouldContain(AgentClientId);
        await permissionDataSeeder.DidNotReceiveWithAnyArgs().SeedAsync(default!, default!, default!, default);
    }

    // Katkiciyi istenen kayitlari tasiyan bellek ici yapilandirmayla kurar.
    private static PermissionGrantDataSeedContributor NewContributor(
        IPermissionDataSeeder permissionDataSeeder,
        out IPermissionDefinitionManager permissionDefinitionManager,
        string? agentPermission = null,
        string? rolePermission = null)
    {
        permissionDefinitionManager = Substitute.For<IPermissionDefinitionManager>();
        permissionDefinitionManager
            .GetPermissionsAsync()
            .Returns(TestModulePermissions.GetAll().Select(CreatePermissionDefinition).ToArray());

        var settings = new Dictionary<string, string?>();
        if (agentPermission is not null)
        {
            settings["AgentClients:Registrations:0:ClientId"] = AgentClientId;
            settings["AgentClients:Registrations:0:Permissions:0"] = agentPermission;
        }

        if (rolePermission is not null)
        {
            settings["RolePermissions:Registrations:0:RoleName"] = AdminRoleName;
            settings["RolePermissions:Registrations:0:Permissions:0"] = rolePermission;
        }

        return new PermissionGrantDataSeedContributor(
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build(),
            permissionDefinitionManager,
            permissionDataSeeder);
    }

    // ABP izin tanimi testte yalnizca Name alaniyla okunur; test alt tipi korumali kurucuyu guvenli acar.
    private static PermissionDefinition CreatePermissionDefinition(string permissionName)
        => new TestPermissionDefinition(permissionName);

    private sealed class TestPermissionDefinition : PermissionDefinition
    {
        public TestPermissionDefinition(string name)
            : base(name)
        {
        }
    }
}
