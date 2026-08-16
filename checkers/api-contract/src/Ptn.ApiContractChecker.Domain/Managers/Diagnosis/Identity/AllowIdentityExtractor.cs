using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Identity;

// islevi: Allow header'indaki HTTP metotlarini hedef path'in snapshot operasyonlariyla dogrular.
// sistemdeki gorevi: Ortam method farkini header metnine guvenmeden katalog olgusuna cevirir.
public sealed class AllowIdentityExtractor : IFailureIdentityExtractor, ITransientDependency
{
    public int Priority => 100;

    // islevi: Standart Allow header'inin sinyalde bulunup bulunmadigini bildirir.
    public bool CanExtract(HttpFailureSignal signal)
        => signal.ResponseHeaders.ContainsKey(DiagnosisHttpConstants.Allow);

    // islevi: Yalniz ayni snapshot path'inde bulunan metotlari kimlige ekler, dogrulanmayanlari atar.
    public void Extract(HttpFailureSignal signal, SpecSnapshotModel snapshot, FailureIdentity identity)
    {
        var documented = snapshot.Operations
            .Where(operation => string.Equals(operation.Path, signal.Path, StringComparison.Ordinal))
            .Select(operation => operation.Method.ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);
        var declared = signal.ResponseHeaders[DiagnosisHttpConstants.Allow]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(method => method.ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(method => method, StringComparer.Ordinal)
            .ToList();
        identity.AllowedMethods = declared.Where(documented.Contains).ToList();
        if (identity.AllowedMethods.Count != declared.Count)
        {
            identity.RejectStructuredName();
            return;
        }

        identity.Upgrade();
    }
}
