using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Identity;

// islevi: WWW-Authenticate challenge gramerinden scheme ve RFC 6750 hata parametrelerini cikarir.
// sistemdeki gorevi: Auth hipotezlerini header metni yerine snapshot'ta dogrulanmis guvenlik semasina baglar.
public sealed class ChallengeIdentityExtractor : IFailureIdentityExtractor, ITransientDependency
{
    public int Priority => 200;

    // islevi: Standart challenge header'inin sinyalde bulunup bulunmadigini bildirir.
    public bool CanExtract(HttpFailureSignal signal)
        => signal.ResponseHeaders.ContainsKey(DiagnosisHttpConstants.WwwAuthenticate);

    // islevi: Scheme'i snapshot security requirement'inda dogrular ve tanili parametreleri kimlige tasir.
    public void Extract(HttpFailureSignal signal, SpecSnapshotModel snapshot, FailureIdentity identity)
    {
        var challenge = signal.ResponseHeaders[DiagnosisHttpConstants.WwwAuthenticate].Trim();
        var separator = challenge.IndexOf(' ');
        var scheme = separator < 0 ? challenge : challenge[..separator];
        if (!IsKnownScheme(snapshot, scheme))
        {
            identity.RejectStructuredName();
            return;
        }

        var parameters = ParseParameters(separator < 0 ? string.Empty : challenge[(separator + 1)..]);
        identity.ChallengeScheme = scheme;
        identity.ChallengeError = parameters.GetValueOrDefault(DiagnosisHttpConstants.ErrorParameter);
        identity.ChallengeScopes = SplitScopes(parameters.GetValueOrDefault(DiagnosisHttpConstants.ScopeParameter));
        identity.Upgrade();
    }

    // islevi: Challenge scheme'ini operasyonlardaki security component adi veya scheme degeriyle dogrular.
    private static bool IsKnownScheme(SpecSnapshotModel snapshot, string scheme)
        => snapshot.Operations
            .SelectMany(operation => operation.SecurityRequirements)
            .SelectMany(requirement => requirement.Schemes)
            .Any(item => string.Equals(item.Name, scheme, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(item.Scheme, scheme, StringComparison.OrdinalIgnoreCase));

    // islevi: Virgul ayiracli auth-param listesini tirnakli degerleri koruyarak sozluge cevirir.
    private static Dictionary<string, string> ParseParameters(string value)
    {
        return SplitOutsideQuotes(value)
            .Select(ParseParameter)
            .Where(item => item.HasValue)
            .Select(item => item.GetValueOrDefault())
            .ToDictionary(item => item.Key, item => item.Value,
                StringComparer.OrdinalIgnoreCase);
    }

    // islevi: Tek auth-param ad/deger ciftini trimleyip tirnaklardan arindirir.
    private static KeyValuePair<string, string>? ParseParameter(string part)
    {
        var separator = part.IndexOf('=');
        if (separator <= 0)
        {
            return null;
        }

        return new KeyValuePair<string, string>(
            part[..separator].Trim(),
            part[(separator + 1)..].Trim().Trim('"'));
    }

    // islevi: Challenge parametrelerini tirnak icindeki virgulleri bolmeden ayirir.
    private static IEnumerable<string> SplitOutsideQuotes(string value)
    {
        var start = 0;
        var quoted = false;
        for (var index = 0; index < value.Length; index++)
        {
            quoted = value[index] == '"' ? !quoted : quoted;
            if (value[index] == ',' && !quoted)
            {
                yield return value[start..index];
                start = index + 1;
            }
        }

        yield return value[start..];
    }

    // islevi: RFC 6750 scope degerini kararli ve tekrarsiz listeye indirger.
    private static List<string> SplitScopes(string? value)
        => (value ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
}
