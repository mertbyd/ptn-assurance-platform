using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Motor bulgularina siddet ve kararli parmak izi metadata'sini uygular.
// sistemdeki gorevi: Ad-hoc ve kalici run akislarinin ayni classifier/calculator ciftini eksiksiz kullanmasini saglar.
/// <summary>
/// Tum bulgu ailelerine siddet ve kararli parmak izi metadata'si ekler.
/// </summary>
public class FindingMetadataEnricher : ITransientDependency
{
    private readonly DifferenceSeverityClassifier _severityClassifier;
    private readonly FindingFingerprintCalculator _fingerprintCalculator;

    /// <summary>
    /// Zenginlestiriciyi siddet siniflandiricisi ve parmak izi hesaplayicisiyla kurar.
    /// </summary>
    public FindingMetadataEnricher(
        DifferenceSeverityClassifier severityClassifier,
        FindingFingerprintCalculator fingerprintCalculator)
    {
        _severityClassifier = severityClassifier;
        _fingerprintCalculator = fingerprintCalculator;
    }

    /// <summary>
    /// Tum bulgu ailelerine ayni motor cifti ve kaynak rolu baglamini uygular.
    /// </summary>
    public void Enrich(
        ComparisonFindings findings,
        string sourceEngineCode,
        string targetEngineCode,
        string sourceRoleCode)
    {
        EnrichSchema(findings, sourceEngineCode, targetEngineCode, sourceRoleCode);
        EnrichData(findings, sourceEngineCode, targetEngineCode, sourceRoleCode);
        EnrichMigrations(findings, sourceEngineCode, targetEngineCode, sourceRoleCode);
    }

    // islevi: Yapisal bulgularin siddet ve fingerprint alanlarini doldurur.
    private void EnrichSchema(
        ComparisonFindings findings,
        string sourceEngineCode,
        string targetEngineCode,
        string sourceRoleCode)
    {
        findings.SchemaDifferences.ForEach(difference =>
        {
            difference.SeverityCode = _severityClassifier.Classify(difference, sourceRoleCode);
            difference.Fingerprint = _fingerprintCalculator.Calculate(
                sourceEngineCode, targetEngineCode, difference);
        });
    }

    // islevi: Veri bulgularinin siddet ve fingerprint alanlarini doldurur.
    private void EnrichData(
        ComparisonFindings findings,
        string sourceEngineCode,
        string targetEngineCode,
        string sourceRoleCode)
    {
        findings.DataDifferences.ForEach(difference =>
        {
            difference.SeverityCode = _severityClassifier.Classify(difference, sourceRoleCode);
            difference.Fingerprint = _fingerprintCalculator.Calculate(
                sourceEngineCode, targetEngineCode, difference);
        });
    }

    // islevi: Migration bulgularinin siddet ve fingerprint alanlarini doldurur.
    private void EnrichMigrations(
        ComparisonFindings findings,
        string sourceEngineCode,
        string targetEngineCode,
        string sourceRoleCode)
    {
        findings.MigrationDifferences.ForEach(difference =>
        {
            difference.SeverityCode = _severityClassifier.Classify(difference, sourceRoleCode);
            difference.Fingerprint = _fingerprintCalculator.Calculate(
                sourceEngineCode, targetEngineCode, difference);
        });
    }
}
