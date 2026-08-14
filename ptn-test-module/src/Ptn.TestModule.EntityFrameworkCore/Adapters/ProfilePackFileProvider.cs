using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;
using Volo.Abp.Settings;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ptn.TestModule.Adapters;

// islevi: Profil paketini ayarli dosya kokunden guvenli bicimde okur, YAML'i cevirir ve SHA-256 ile muhurlar.
// sistemdeki gorevi: Domain profil portunu tablo veya yeni katman acmadan dosya tabanli adapter olarak uygular.
public class ProfilePackFileProvider : IProfilePackProvider
{
    private readonly ISettingProvider _settingProvider;
    private readonly IHostEnvironment _hostEnvironment;

    // Ayar ve host kokunu guvenli profil dosyasi cozumlemesi icin baglar.
    public ProfilePackFileProvider(
        ISettingProvider settingProvider,
        IHostEnvironment hostEnvironment)
    {
        _settingProvider = settingProvider;
        _hostEnvironment = hostEnvironment;
    }

    // Profil dosyasini boyut butcesiyle okur, semasini cevirir ve icerik fingerprint'ini ekler.
    public async Task<PtnProfilePack> LoadAsync(
        string profileKey,
        CancellationToken cancellationToken)
    {
        EnsureProfileKeyIsSafe(profileKey);
        var filePath = await ResolveFilePathAsync(profileKey);
        var bytes = await ReadProfileBytesAsync(filePath, cancellationToken);
        var document = Deserialize(bytes);
        var pack = Map(document);
        EnsureProfileMatchesRequest(pack, profileKey);
        pack.ContentFingerprint = ComputeFingerprint(bytes);
        return pack;
    }

    // Ayarli kok yolu host kokune baglar ve profil dosyasinin kok disina cikmasini engeller.
    private async Task<string> ResolveFilePathAsync(string profileKey)
    {
        var configured = await _settingProvider.GetOrNullAsync(PtnBridgeSettingNames.ProfilePackPath)
                         ?? PtnBridgeSettingNames.DefaultProfilePackPath;
        var root = Path.IsPathRooted(configured)
            ? Path.GetFullPath(configured)
            : Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configured));
        var filePath = Path.GetFullPath(Path.Combine(
            root,
            profileKey + PtnBridgeSettingNames.ProfilePackExtension));
        var rootedPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!filePath.StartsWith(rootedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }

        return filePath;
    }

    // Dosyayi tek handle uzerinden okur ve okumadan once profil boyut butcesini uygular.
    private static async Task<byte[]> ReadProfileBytesAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackNotFound);
        }

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length <= 0 || stream.Length > PtnBridgeConsts.MaxProfilePackBytes)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }

        var bytes = new byte[(int)stream.Length];
        await stream.ReadExactlyAsync(bytes, cancellationToken);
        return bytes;
    }

    // YAML belgesini bilinmeyen alanlari reddeden dar transport modeline cevirir.
    private static ProfilePackDocument Deserialize(byte[] bytes)
    {
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            using var reader = new StreamReader(new MemoryStream(bytes));
            return deserializer.Deserialize<ProfilePackDocument>(reader);
        }
        catch (YamlException exception)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid, innerException: exception);
        }
    }

    // YAML transport modelini davranissiz domain profil modeline cevirir.
    private static PtnProfilePack Map(ProfilePackDocument document)
    {
        return new PtnProfilePack
        {
            ProfileKey = document.ProfileKey,
            Revision = document.Revision,
            DbSchemaFingerprint = document.DbSchemaFingerprint,
            SpecSnapshotId = document.SpecSnapshotId,
            Bindings = document.Bindings.Select(MapBinding).ToList(),
            Paths = document.Paths.Select(MapPath).ToList()
        };
    }

    // Tek YAML kavram baglamasini domain veri kabuguna cevirir.
    private static PtnConceptBinding MapBinding(ConceptBindingDocument document)
    {
        return new PtnConceptBinding
        {
            ConceptCode = document.ConceptCode,
            DbSchemaName = document.DbSchemaName,
            TableName = document.TableName,
            ColumnMap = document.ColumnMap,
            PatternCode = document.PatternCode,
            StateCode = document.StateCode,
            ApprovedBy = document.ApprovedBy
        };
    }

    // Tek YAML kanit yolunu tetikleyici, adim ve hukum alanlariyla domain modeline cevirir.
    private static PtnEvidencePathDefinition MapPath(EvidencePathDocument document)
    {
        return new PtnEvidencePathDefinition
        {
            PathKey = document.PathKey,
            Trigger = new PtnEvidencePathTrigger
            {
                StatusCodes = document.Trigger.StatusCodes,
                OperationIds = document.Trigger.OperationIds
            },
            Steps = document.Steps.Select(MapStep).ToList(),
            ConfirmedWhen = document.ConfirmedWhen,
            InconclusiveWhen = document.InconclusiveWhen
        };
    }

    // Tek YAML kanit adimini kapali kaynak ve dugum kodlariyla domain modeline cevirir.
    private static PtnEvidencePathStep MapStep(EvidenceStepDocument document)
    {
        return new PtnEvidencePathStep
        {
            NodeKindCode = document.NodeKind,
            SourceCode = document.Source,
            ConceptCode = document.Concept,
            JoinFromNodeKindCode = document.JoinFrom,
            Parameters = document.Parameters
        };
    }

    // Profil anahtarini yalniz dosya-adi guvenli kapali karakterlerle sinirlar.
    private static void EnsureProfileKeyIsSafe(string profileKey)
    {
        var safe = !string.IsNullOrWhiteSpace(profileKey) && profileKey.All(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' or '.');
        if (!safe)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }
    }

    // Dosya icerigi ile YAML icindeki profil anahtarinin ayni kaydi gostermesini zorunlu kilar.
    private static void EnsureProfileMatchesRequest(PtnProfilePack pack, string profileKey)
    {
        if (pack.ProfileKey != profileKey)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }
    }

    // Ham profil baytlarini lowercase sha256: sozlesmesine cevirir.
    private static string ComputeFingerprint(byte[] bytes)
    {
        return PtnBridgeSettingNames.FingerprintPrefix +
               Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    // islevi: YAML profil belgesinin kok transport seklini tasir.
    // sistemdeki gorevi: Dis dosya semasini domain modelinden ayri ve dar tutar.
    private sealed class ProfilePackDocument
    {
        public string ProfileKey { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string DbSchemaFingerprint { get; set; } = string.Empty;
        public Guid? SpecSnapshotId { get; set; }
        public List<ConceptBindingDocument> Bindings { get; set; } = [];
        public List<EvidencePathDocument> Paths { get; set; } = [];
    }

    // islevi: YAML icindeki tek kavram baglamasinin transport alanlarini tasir.
    // sistemdeki gorevi: Dosya alan adlarini domain baglama modeline kontrollu cevirir.
    private sealed class ConceptBindingDocument
    {
        public string ConceptCode { get; set; } = string.Empty;
        public string DbSchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public Dictionary<string, string> ColumnMap { get; set; } = new(StringComparer.Ordinal);
        public string PatternCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
    }

    // islevi: YAML icindeki tek kanit yolunun transport alanlarini tasir.
    // sistemdeki gorevi: Yol verisini serbest nesne yerine bilinen semayla sinirlar.
    private sealed class EvidencePathDocument
    {
        public string PathKey { get; set; } = string.Empty;
        public EvidenceTriggerDocument Trigger { get; set; } = new();
        public List<EvidenceStepDocument> Steps { get; set; } = [];
        public string ConfirmedWhen { get; set; } = string.Empty;
        public string InconclusiveWhen { get; set; } = string.Empty;
    }

    // islevi: YAML yol tetikleyicisinin status ve operasyon alanlarini tasir.
    // sistemdeki gorevi: Yol secimini kapali tetikleyici listeleriyle sinirlar.
    private sealed class EvidenceTriggerDocument
    {
        public List<int> StatusCodes { get; set; } = [];
        public List<string> OperationIds { get; set; } = [];
    }

    // islevi: YAML kanit adiminin kaynak, dugum ve baglama alanlarini tasir.
    // sistemdeki gorevi: Adim cevirisini serbest koddan kapali domain sozlugune baglar.
    private sealed class EvidenceStepDocument
    {
        public string NodeKind { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? Concept { get; set; }
        public string? JoinFrom { get; set; }
        public Dictionary<string, string?> Parameters { get; set; } = new(StringComparer.Ordinal);
    }
}
