using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>Tenant ayarindaki tum ortam baglamalarini sir degerleri acilmadan cozer.</summary>
    public async Task<IReadOnlyList<TestRunEnvironmentBinding>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var configured = await _settingProvider.GetOrNullAsync(TestModuleRunSettingNames.EnvironmentBindings);
        if (string.IsNullOrWhiteSpace(configured))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(configured);
            var bindings = new List<TestRunEnvironmentBinding>();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                bindings.Add(ReadBinding(property.Value, property.Name));
            }

            return bindings.OrderBy(item => item.EnvironmentKey, StringComparer.Ordinal).ToList();
        }
        catch (JsonException exception)
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound, innerException: exception);
        }
    }

    // Yeni mantiksal ortami haritaya ekler; ayni anahtar zaten bagliysa reddeder.
    /// <summary>Yeni ortam baglamasini iceren kararli setting belgesini uretir.</summary>
    public string Bind(string? configured, TestRunEnvironmentBinding binding)
    {
        Normalize(binding, binding.EnvironmentKey);
        var map = ReadMap(configured);
        if (map.ContainsKey(binding.EnvironmentKey))
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentAlreadyBound);
        }

        map[binding.EnvironmentKey] = CreateEntry(binding);
        return Serialize(map);
    }

    // Bagli bir ortamin hedeflerini degistirir; mantiksal anahtar cagiran taraftan gelir ve degismez.
    /// <summary>Bagli ortamin hedeflerini degistiren kararli setting belgesini uretir.</summary>
    public string Rebind(string? configured, string environmentKey, TestRunEnvironmentBinding binding)
    {
        Normalize(binding, environmentKey);
        var map = ReadMap(configured);
        EnsureKeyIsBound(map, binding.EnvironmentKey);
        map[binding.EnvironmentKey] = CreateEntry(binding);
        return Serialize(map);
    }

    // Bagli ortami haritadan cikarir; bagli olmayan anahtar reddedilir.
    /// <summary>Verilen ortami cikaran kararli setting belgesini uretir.</summary>
    public string Unbind(string? configured, string environmentKey)
    {
        var key = NormalizeKey(environmentKey);
        var map = ReadMap(configured);
        EnsureKeyIsBound(map, key);
        map.Remove(key);
        return Serialize(map);
    }

    // Setting belgesini anahtara gore siralanmis calisma haritasina cevirir.
    /// <summary>Mevcut setting degerini kararli sirali ortam haritasina cozer.</summary>
    private static SortedDictionary<string, JsonNode> ReadMap(string? configured)
    {
        var map = new SortedDictionary<string, JsonNode>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(configured))
        {
            return map;
        }

        try
        {
            var document = JsonNode.Parse(configured) as JsonObject;
            foreach (var property in document ?? throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound))
            {
                map[property.Key] = property.Value?.DeepClone()
                    ?? throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
            }

            return map;
        }
        catch (JsonException exception)
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound, innerException: exception);
        }
    }

    // Yazilacak baglamayi mantiksal anahtariyla birlikte normalize eder ve iki hedefi de dogrular.
    /// <summary>Baglama modelini kalici bicime normalize eder ve yazilabilir oldugunu dogrular.</summary>
    private static void Normalize(TestRunEnvironmentBinding binding, string environmentKey)
    {
        binding.EnvironmentKey = NormalizeKey(environmentKey);
        binding.BaseUrl = EnsureAbsoluteBaseUrl(binding.BaseUrl);
        binding.SecretRef = binding.SecretRef?.Trim() ?? string.Empty;
        binding.ApiSecretRef = binding.ApiSecretRef?.Trim() ?? string.Empty;
        EnsureTargetsAreIdentified(binding);
    }

    // Normalize edilmis hedefleri ortak anahtarli api ve database bolumlerine yazar.
    /// <summary>Tek ortam satirini kararli JSON nesnesi olarak kurar.</summary>
    private static JsonNode CreateEntry(TestRunEnvironmentBinding binding)
    {
        var api = new JsonObject
        {
            [TestModuleRunSettingNames.EnvironmentKey] = binding.EnvironmentKey,
            [TestModuleRunSettingNames.BaseUrl] = binding.BaseUrl,
            [TestModuleRunSettingNames.SpecSnapshotId] = binding.SpecSnapshotId.ToString()
        };
        if (binding.ApiSecretRef.Length > 0)
        {
            api[TestModuleRunSettingNames.SecretRef] = binding.ApiSecretRef;
        }

        return new JsonObject
        {
            [TestModuleRunSettingNames.ApiSection] = api,
            [TestModuleRunSettingNames.DatabaseSection] = new JsonObject
            {
                [TestModuleRunSettingNames.EnvironmentKey] = binding.EnvironmentKey,
                [TestModuleRunSettingNames.DbConnectionId] = binding.DbConnectionId.ToString(),
                [TestModuleRunSettingNames.SecretRef] = binding.SecretRef
            }
        };
    }

    // Haritayi her zaman ayni anahtar sirasiyla tek setting metnine cevirir.
    /// <summary>Ortam haritasini kararli siralamayla setting metnine cevirir.</summary>
    private static string Serialize(SortedDictionary<string, JsonNode> map)
    {
        var document = new JsonObject();
        foreach (var entry in map)
        {
            document[entry.Key] = entry.Value;
        }

        return document.ToJsonString();
    }

    // Mantiksal ortam anahtarini normalize eder; bos anahtar baglanamaz.
    /// <summary>Yazilacak mantiksal ortam anahtarini normalize eder.</summary>
    private static string NormalizeKey(string? environmentKey)
    {
        var key = environmentKey?.Trim();
        return !string.IsNullOrWhiteSpace(key)
            ? key
            : throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
    }

    // Guncelleme ve silme yalniz zaten bagli bir ortam uzerinde calisir.
    /// <summary>Verilen anahtarin haritada bagli oldugunu dogrular.</summary>
    private static void EnsureKeyIsBound(SortedDictionary<string, JsonNode> map, string environmentKey)
    {
        if (!map.ContainsKey(environmentKey))
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentNotBound);
        }
    }

    // Runner ve checker'lar goreli adres cozemez; taban adres mutlak olmak zorundadir.
    /// <summary>Taban adresin mutlak http veya https adresi oldugunu dogrular.</summary>
    private static string EnsureAbsoluteBaseUrl(string baseUrl)
    {
        var candidate = baseUrl?.Trim();
        var isAbsolute = Uri.TryCreate(candidate, UriKind.Absolute, out var address) &&
                         (address.Scheme == Uri.UriSchemeHttp || address.Scheme == Uri.UriSchemeHttps);
        return isAbsolute
            ? candidate!
            : throw new BusinessException(TestModuleRunErrorCodes.EnvironmentBaseUrlInvalid);
    }

    // Iki checker hedefi de kimliklenmeden ortam kosulamaz.
    /// <summary>Snapshot, baglanti ve secret referansinin bos olmadigini dogrular.</summary>
    private static void EnsureTargetsAreIdentified(TestRunEnvironmentBinding binding)
    {
        var identified = binding.SpecSnapshotId != Guid.Empty &&
                         binding.DbConnectionId != Guid.Empty &&
                         !string.IsNullOrWhiteSpace(binding.SecretRef);
        if (!identified)
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentTargetInvalid);
        }
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
            SecretRef = GetRequiredString(database, TestModuleRunSettingNames.SecretRef),
            ApiSecretRef = GetOptionalString(api, TestModuleRunSettingNames.SecretRef)
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

    // Korumasiz ucler icin secret referansi opsiyoneldir; yoklugu kosumu engellemez.
    /// <summary>Verilen alan adindaki opsiyonel JSON metnini getirir.</summary>
    private static string GetOptionalString(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return string.Empty;
        }

        return value.GetString()?.Trim() ?? string.Empty;
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
