using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Managers.Comparison;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Runs;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Runs;

// islevi: Comparison bulgularinin eski ve yeni degerlerini secilen retention politikasindan gecirir.
// sistemdeki gorevi: Saf kiyaslama motoruna dokunmadan kalicilik sinirinda deger sizmasini kapatir.
public class FindingValueRetentionManager : ITransientDependency
{
    private readonly FindingValueRedactor _redactor;
    private readonly FindingFingerprintCalculator _fingerprintCalculator;

    public FindingValueRetentionManager(
        FindingValueRedactor redactor,
        FindingFingerprintCalculator fingerprintCalculator)
    {
        _redactor = redactor;
        _fingerprintCalculator = fingerprintCalculator;
    }

    // islevi: Tum finding degerlerini ayni politikayla yeni owned JSON govdesine indirger.
    public ContractCheckFindings Apply(ContractCheckFindings findings, ValueRetentionPolicy policy)
    {
        var retained = new List<Finding>(findings.Items.Count);
        foreach (var finding in findings.Items)
        {
            var oldValue = _redactor.Redact(finding.OldValue, policy);
            var newValue = _redactor.Redact(finding.NewValue, policy);
            var fingerprint = _fingerprintCalculator.Calculate(finding, oldValue, newValue, policy.ModeCode);
            retained.Add(new Finding(
                finding.KindCode,
                finding.SeverityCode,
                finding.DirectionCode,
                finding.Address,
                oldValue,
                newValue,
                fingerprint));
        }

        return new ContractCheckFindings(retained);
    }
}
