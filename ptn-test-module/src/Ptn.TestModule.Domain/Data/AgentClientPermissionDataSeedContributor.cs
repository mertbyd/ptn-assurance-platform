using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Ptn.TestModule.Constants;
using Ptn.TestModule.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace Ptn.TestModule.Data;

// islevi: Yapilandirmada tanimlanan makine istemcilerine bu modulun izinlerini idempotent olarak verir.
// sistemdeki gorevi: client-credentials token'inin kullanicisi yoktur; yetki ABP'nin "C" saglayicisiyla
// client_id uzerinden okunur, dolayisiyla grant'i host tarafinda birinin yazmasi gerekir (ADR-0013).
public class AgentClientPermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly IPermissionDataSeeder _permissionDataSeeder;

    public AgentClientPermissionDataSeedContributor(
        IConfiguration configuration,
        IPermissionDataSeeder permissionDataSeeder)
    {
        _configuration = configuration;
        _permissionDataSeeder = permissionDataSeeder;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        var registrations = _configuration
            .GetSection(TestModuleConfigurationKeys.AgentClientRegistrations)
            .GetChildren();

        foreach (var registration in registrations)
        {
            var clientId = registration[TestModuleConfigurationKeys.AgentClientId];
            if (string.IsNullOrWhiteSpace(clientId))
            {
                continue;
            }

            var permissionNames = registration
                .GetSection(TestModuleConfigurationKeys.AgentClientPermissions)
                .Get<string[]>() ?? [];

            EnsureModulePermissions(clientId, permissionNames);
            await _permissionDataSeeder.SeedAsync(
                ClientPermissionValueProvider.ProviderName,
                clientId,
                permissionNames,
                context.TenantId);
        }
    }

    /* Yalniz bu modulun izinleri verilebilir: yapilandirmaya yazilan yabanci bir izin adi, makine
     * istemcisini sessizce baska bir modulun yuzeyine acardi. Bilinmeyen ad da reddedilir; aksi halde
     * yazim hatasi hicbir grant uretmez ve sebebi anlasilmayan 403 olarak geri doner. */
    private static void EnsureModulePermissions(string clientId, IReadOnlyCollection<string> permissionNames)
    {
        var knownPermissions = TestModulePermissions.GetAll().ToHashSet(StringComparer.Ordinal);
        var unknownPermissions = permissionNames
            .Where(permissionName => !knownPermissions.Contains(permissionName))
            .ToList();

        if (unknownPermissions.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"{TestModuleConfigurationKeys.AgentClientRegistrations} '{clientId}' istemcisi icin " +
            $"bu modulde tanimli olmayan izin adlari tasiyor: {string.Join(", ", unknownPermissions)}.");
    }
}
