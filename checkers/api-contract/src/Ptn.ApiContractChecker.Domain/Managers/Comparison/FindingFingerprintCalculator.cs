using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Comparison;

// islevi: Retention uygulanmis bulgu kimligini adres ve delta uzerinden SHA256 ile hesaplar.
// sistemdeki gorevi: Kosudan bagimsiz bulgu eslesmesini comparison motorunun kural kataloguna dokunmadan saglar.
public sealed class FindingFingerprintCalculator : ITransientDependency
{
    // islevi: Kod, sabit sirali adres ve retention uyumlu deltayi tek kararli hash girdisine indirger.
    public string Calculate(
        Finding finding,
        string? retainedOldValue,
        string? retainedNewValue,
        string retentionModeCode)
    {
        var components = new List<string>
        {
            finding.KindCode,
            finding.DirectionCode
        };
        AddAddress(components, finding.Address);
        components.Add(NormalizeDeltaValue(finding.OldValue, retainedOldValue, retentionModeCode));
        components.Add(NormalizeDeltaValue(finding.NewValue, retainedNewValue, retentionModeCode));
        var framed = new List<string>(components.Count);
        foreach (var component in components)
        {
            framed.Add(Frame(component));
        }

        var payload = string.Join(ContractCheckRunConsts.FingerprintSeparator, framed);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    // islevi: FindingAddress'in sekiz bilesenini degismeyen sirayla hash girdisine ekler.
    private static void AddAddress(ICollection<string> components, FindingAddress address)
    {
        foreach (var component in FindingAddressGrammar.BuildComponents(
                     address.OperationId,
                     address.HttpMethod,
                     address.Path,
                     address.SchemaName,
                     address.PropertyPath,
                     address.ParameterName,
                     address.ResponseStatus,
                     address.MediaType))
        {
            components.Add(component);
        }
    }

    // islevi: None politikasinda ham deger yerine yalniz varlik ve JSON tip damgasini kullanir.
    private static string NormalizeDeltaValue(string? rawValue, string? retainedValue, string modeCode)
    {
        if (modeCode == ValueRetentionModeCodes.None)
        {
            return rawValue is null ? ContractCheckRunConsts.FingerprintMissingValue : GetTypeStamp(rawValue);
        }

        return retainedValue is null
            ? ContractCheckRunConsts.FingerprintMissingValue
            : ContractCheckRunConsts.FingerprintValuePrefix + retainedValue;
    }

    // islevi: Ham metni rapora tasimadan JSON deger turunu kararli damgaya cevirir.
    private static string GetTypeStamp(string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            return ContractCheckRunConsts.FingerprintValuePrefix + document.RootElement.ValueKind;
        }
        catch (JsonException)
        {
            return ContractCheckRunConsts.FingerprintValuePrefix + JsonValueKind.String;
        }
    }

    // islevi: Ayrac iceren degerlerde birlesim carpismasini uzunluk on ekiyle engeller.
    private static string Frame(string value) => FindingAddressGrammar.Frame(value);
}
