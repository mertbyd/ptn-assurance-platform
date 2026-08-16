using System;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.EntityFrameworkCore.Configurations;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery;

// islevi: External katalog DbContext'lerinin kendi konfigurasyonlarini assembly icinden guvenli filtreyle otomatik uygulamasini saglar.
// sistemdeki gorevi: Ana app DbContext'ine katalog mapping'leri sizmadan, her katalog context'i yalniz kendi provider namespace'indeki ICatalogModelConfiguration dosyalarini alir; yeni catalog row/config eklenince DbContext'te elle liste guncellenmez.
internal static class CatalogDbContextModelCreatingExtensions
{
    // islevi: Verilen konfigurasyon tipinin namespace'indeki katalog konfigurasyonlarini model uzerine uygular.
    internal static void ApplyCatalogConfigurationsFromNamespace(
        this ModelBuilder modelBuilder,
        Type configurationNamespaceMarker)
    {
        var configurationNamespace = configurationNamespaceMarker.Namespace;

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContextModelCreatingExtensions).Assembly,
            type => typeof(ICatalogModelConfiguration).IsAssignableFrom(type) &&
                    type.Namespace == configurationNamespace);
    }
}
