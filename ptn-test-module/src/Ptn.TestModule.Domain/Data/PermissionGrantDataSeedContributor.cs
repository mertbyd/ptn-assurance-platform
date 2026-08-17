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

// islevi: Yapilandirmada tanimlanan makine istemcilerine ve yonetici rollerine bu modulun izinlerini idempotent verir.
// sistemdeki gorevi: Izin tanimlari bu hostta yuklenir, dolayisiyla grant'i de bu host yazar; issuer seed'i yalniz
// kendi hostunda gorunen izinleri dagitabilir (ADR-0013).
public class PermissionGrantDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly IPermissionDataSeeder _permissionDataSeeder;

    public PermissionGrantDataSeedContributor(
        IConfiguration configuration,
        IPermissionDataSeeder permissionDataSeeder)
    {
        _configuration = configuration;
        _permissionDataSeeder = permissionDataSeeder;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        /* client-credentials token'inin kullanicisi yoktur; ABP yetkiyi client saglayicisiyla client_id
         * uzerinden okur. Insan kullanicisi ise rolu uzerinden okunur. Iki kayit da ayni bicimdedir,
         * yalnizca saglayici ve anahtar alani degisir. */
        await SeedRegistrationsAsync(
            TestModuleConfigurationKeys.AgentClientRegistrations,
            TestModuleConfigurationKeys.AgentClientId,
            ClientPermissionValueProvider.ProviderName,
            context.TenantId);
        await SeedRegistrationsAsync(
            TestModuleConfigurationKeys.RolePermissionRegistrations,
            TestModuleConfigurationKeys.RolePermissionRoleName,
            RolePermissionValueProvider.ProviderName,
            context.TenantId);
    }

    // Verilen bolumdeki her kaydi dogrulayip tek saglayici altina yazar.
    private async Task SeedRegistrationsAsync(
        string sectionKey,
        string providerKeyName,
        string providerName,
        Guid? tenantId)
    {
        foreach (var registration in _configuration.GetSection(sectionKey).GetChildren())
        {
            var providerKey = registration[providerKeyName];
            if (string.IsNullOrWhiteSpace(providerKey))
            {
                continue;
            }

            var permissionNames = registration
                .GetSection(TestModuleConfigurationKeys.GrantedPermissions)
                .Get<string[]>() ?? [];

            EnsureModulePermissions(sectionKey, providerKey, permissionNames);
            await _permissionDataSeeder.SeedAsync(providerName, providerKey, permissionNames, tenantId);
        }
    }

    /* Yalniz bu modulun izinleri verilebilir: yapilandirmaya yazilan yabanci bir izin adi, alici tarafi
     * sessizce baska bir modulun yuzeyine acardi. Bilinmeyen ad da reddedilir; aksi halde yazim hatasi
     * hicbir grant uretmez ve sebebi anlasilmayan 403 olarak geri doner. */
    private static void EnsureModulePermissions(
        string sectionKey,
        string providerKey,
        IReadOnlyCollection<string> permissionNames)
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
            $"{sectionKey} '{providerKey}' kaydi bu modulde tanimli olmayan izin adlari tasiyor: " +
            $"{string.Join(", ", unknownPermissions)}.");
    }
}
