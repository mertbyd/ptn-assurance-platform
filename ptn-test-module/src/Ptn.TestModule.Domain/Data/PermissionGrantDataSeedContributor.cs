using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Ptn.TestModule.Constants;
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
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;

    public PermissionGrantDataSeedContributor(
        IConfiguration configuration,
        IPermissionDefinitionManager permissionDefinitionManager,
        IPermissionDataSeeder permissionDataSeeder)
    {
        _configuration = configuration;
        _permissionDefinitionManager = permissionDefinitionManager;
        _permissionDataSeeder = permissionDataSeeder;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        var definedPermissionNames = (await _permissionDefinitionManager.GetPermissionsAsync())
            .Select(permission => permission.Name)
            .ToHashSet(StringComparer.Ordinal);

        /* client-credentials token'inin kullanicisi yoktur; ABP yetkiyi client saglayicisiyla client_id
         * uzerinden okur. Insan kullanicisi ise rolu uzerinden okunur. Iki kayit da ayni bicimdedir,
         * yalnizca saglayici ve anahtar alani degisir. */
        await SeedRegistrationsAsync(
            TestModuleConfigurationKeys.AgentClientRegistrations,
            TestModuleConfigurationKeys.AgentClientId,
            ClientPermissionValueProvider.ProviderName,
            definedPermissionNames,
            context.TenantId);
        await SeedRegistrationsAsync(
            TestModuleConfigurationKeys.RolePermissionRegistrations,
            TestModuleConfigurationKeys.RolePermissionRoleName,
            RolePermissionValueProvider.ProviderName,
            definedPermissionNames,
            context.TenantId);
    }

    // Verilen bolumdeki her kaydi dogrulayip tek saglayici altina yazar.
    private async Task SeedRegistrationsAsync(
        string sectionKey,
        string providerKeyName,
        string providerName,
        IReadOnlySet<string> definedPermissionNames,
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

            EnsureComposedHostPermissions(sectionKey, providerKey, permissionNames, definedPermissionNames);
            await _permissionDataSeeder.SeedAsync(providerName, providerKey, permissionNames, tenantId);
        }
    }

    /* Bu composition hostunda gercekten yuklenen bir izin verilebilir. Boylece checker ve Emailing gibi
     * compose edilen modullerin izinleri kabul edilir; yazim hatasi veya hostta bulunmayan bir izin ise
     * sessiz 403 uretmeden baslangicta reddedilir. */
    private static void EnsureComposedHostPermissions(
        string sectionKey,
        string providerKey,
        IReadOnlyCollection<string> permissionNames,
        IReadOnlySet<string> definedPermissionNames)
    {
        var unknownPermissions = permissionNames
            .Where(permissionName => !definedPermissionNames.Contains(permissionName))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (unknownPermissions.Length == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"{sectionKey} '{providerKey}' kaydi bu hostta tanimli olmayan izin adlari tasiyor: " +
            $"{string.Join(", ", unknownPermissions)}.");
    }
}
