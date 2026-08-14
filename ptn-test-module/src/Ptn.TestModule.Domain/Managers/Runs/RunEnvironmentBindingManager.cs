using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Tenant-scoped ortam ayarini cozer ve API ile DB hedefinin ayni ortama bagli oldugunu dogrular.
// sistemdeki gorevi: Yanlis API-DB eslesmesini kosum baslamadan once reddeden tek domain sahibidir.
/// <summary>
/// Mantiksal test ortamlarini ABP Setting degerinden guvenli binding modeline cozer.
/// </summary>
public class RunEnvironmentBindingManager : TestModuleDomainService
{
    /// <summary>Aktif tenant'in ABP setting degerlerini okuyan provider'dir.</summary>
    private readonly ISettingProvider _settingProvider;

    // Tenant-aware setting provider'ini ortam cozumleme kapisina baglar.
    /// <summary>Ortam baglama manager'ini aktif setting provider ile kurar.</summary>
    public RunEnvironmentBindingManager(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    // Mantiksal ortami cozer ve iki hedefin environmentKey degerlerini birebir eslestirir.
    /// <summary>Verilen mantiksal ortam icin dogrulanmis API ve veritabani baglamasini getirir.</summary>
    public async Task<TestRunEnvironmentBinding> ResolveAsync(
        string environmentKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestedKey = environmentKey?.Trim();
        if (string.IsNullOrWhiteSpace(requestedKey))
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
        }

        var configured = await _settingProvider.GetOrNullAsync(TestModuleRunSettingNames.EnvironmentBindings);
        return Resolve(configured, requestedKey);
    }

    // JSON haritasindan istenen ortam satirini guvenli tiplerle cozer.
    /// <summary>Setting JSON degerinden istenen ortam baglamasini olusturur.</summary>
    private static TestRunEnvironmentBinding Resolve(string? configured, string requestedKey)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
        }

        try
        {
            using var document = JsonDocument.Parse(configured);
            if (!document.RootElement.TryGetProperty(requestedKey, out var environment))
            {
                throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
            }

            return ReadBinding(environment, requestedKey);
        }
        catch (JsonException exception)
        {
            throw new BusinessException(
                TestModuleRunErrorCodes.EnvironmentNotBound,
                innerException: exception);
        }
    }

    // API ve database bolumlerini okuyup ortak environmentKey kapisini uygular.
    /// <summary>Tek ortam JSON nesnesini dogrulanmis domain binding modeline cevirir.</summary>
    private static TestRunEnvironmentBinding ReadBinding(
        JsonElement environment,
        string requestedKey)
    {
        var api = GetRequiredObject(environment, TestModuleRunSettingNames.ApiSection);
        var database = GetRequiredObject(environment, TestModuleRunSettingNames.DatabaseSection);
        var apiEnvironmentKey = GetRequiredString(api, TestModuleRunSettingNames.EnvironmentKey);
        var databaseEnvironmentKey = GetRequiredString(database, TestModuleRunSettingNames.EnvironmentKey);
        EnsureEnvironmentMatches(requestedKey, apiEnvironmentKey, databaseEnvironmentKey);

        return new TestRunEnvironmentBinding
        {
            EnvironmentKey = requestedKey,
            BaseUrl = GetRequiredString(api, TestModuleRunSettingNames.BaseUrl),
            SpecSnapshotId = GetRequiredGuid(api, TestModuleRunSettingNames.SpecSnapshotId),
            DbConnectionId = GetRequiredGuid(database, TestModuleRunSettingNames.DbConnectionId),
            SecretRef = GetRequiredString(database, TestModuleRunSettingNames.SecretRef)
        };
    }

    // Iki hedefin ve istenen mantiksal adin ayni environmentKey'i tasimasini zorunlu kilar.
    /// <summary>API ve veritabani ortam anahtarlarinin istenen anahtarla ayni oldugunu dogrular.</summary>
    private static void EnsureEnvironmentMatches(
        string requestedKey,
        string apiEnvironmentKey,
        string databaseEnvironmentKey)
    {
        var matches = string.Equals(requestedKey, apiEnvironmentKey, StringComparison.Ordinal) &&
                      string.Equals(apiEnvironmentKey, databaseEnvironmentKey, StringComparison.Ordinal);
        if (!matches)
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentMismatch);
        }
    }

    // Zorunlu JSON alt nesnesini bulur.
    /// <summary>Verilen alan adindaki zorunlu JSON nesnesini getirir.</summary>
    private static JsonElement GetRequiredObject(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Object)
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
        }

        return value;
    }

    // Zorunlu ve bos olmayan JSON metin alanini okur.
    /// <summary>Verilen alan adindaki zorunlu JSON metnini getirir.</summary>
    private static string GetRequiredString(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
        }

        var text = value.GetString();
        return !string.IsNullOrWhiteSpace(text)
            ? text.Trim()
            : throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
    }

    // Zorunlu JSON Guid alanini bos olmayan kimlige cevirir.
    /// <summary>Verilen alan adindaki zorunlu ve bos olmayan Guid kimligini getirir.</summary>
    private static Guid GetRequiredGuid(JsonElement source, string propertyName)
    {
        var text = GetRequiredString(source, propertyName);
        return Guid.TryParse(text, out var id) && id != Guid.Empty
            ? id
            : throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
    }
}
