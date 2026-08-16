using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Runs.Reports;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Runs;

// islevi: Contract check findings govdesinden sayacli ve gruplu raporu deterministik olarak uretir.
// sistemdeki gorevi: Kalici report kolonu olmadan saf rapor hesaplamasini run yasam dongusu manager'indan ayirir.
public class ContractCheckReportBuilder : ITransientDependency
{
    // Findings govdesinden severity, direction, kind sayaclari ve kind gruplarini deterministik uretir.
    public ContractCheckReportAggregationModel Build(ContractCheckFindings findings)
    {
        var severityCounts = CountByCode(findings.Items, finding => finding.SeverityCode);
        var directionCounts = CountByCode(findings.Items, finding => finding.DirectionCode);
        var kindCounts = CountByCode(findings.Items, finding => finding.KindCode);

        return new ContractCheckReportAggregationModel
        {
            Summary = new ContractCheckReportSummaryModel
            {
                TotalFindingCount = findings.Items.Count,
                BreakingCount = GetCount(severityCounts, DifferenceSeverityCodes.Breaking),
                NonBreakingCount = GetCount(severityCounts, DifferenceSeverityCodes.NonBreaking),
                DocsOnlyCount = GetCount(severityCounts, DifferenceSeverityCodes.DocsOnly),
                SeverityCounts = BuildCounts(DifferenceSeverityCodes.All, severityCounts),
                DirectionCounts = BuildCounts(DifferenceDirectionCodes.All, directionCounts),
                KindCounts = BuildCounts(DifferenceKindCodes.All, kindCounts)
            },
            Groups = BuildGroups(findings.Items)
        };
    }

    // Findings listesindeki kararli kod tekrarlarini tek geciste sayar.
    private static Dictionary<string, int> CountByCode(
        IReadOnlyCollection<Finding> findings,
        Func<Finding, string> codeSelector)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var finding in findings)
        {
            var code = codeSelector(finding);
            counts[code] = GetCount(counts, code) + 1;
        }

        return counts;
    }

    // Kapali katalog sirasini koruyarak yalniz findings'te bulunan count modellerini kurar.
    private static List<ContractCheckReportCountModel> BuildCounts(
        IReadOnlyCollection<string> knownCodes,
        IReadOnlyDictionary<string, int> counts)
    {
        var result = new List<ContractCheckReportCountModel>(counts.Count);
        foreach (var code in knownCodes)
        {
            var count = GetCount(counts, code);
            if (count == 0)
            {
                continue;
            }

            result.Add(new ContractCheckReportCountModel
            {
                Code = code,
                Count = count
            });
        }

        return result;
    }

    // Yalniz bulunan kind kodlari icin katalog sirasinda bulgu gruplari kurar.
    private static List<ContractCheckReportGroupModel> BuildGroups(IReadOnlyCollection<Finding> findings)
    {
        var grouped = new Dictionary<string, List<Finding>>(StringComparer.Ordinal);
        foreach (var finding in findings)
        {
            if (!grouped.TryGetValue(finding.KindCode, out var groupFindings))
            {
                groupFindings = [];
                grouped.Add(finding.KindCode, groupFindings);
            }

            groupFindings.Add(finding);
        }

        var groups = new List<ContractCheckReportGroupModel>(grouped.Count);
        foreach (var kindCode in DifferenceKindCodes.All)
        {
            if (!grouped.TryGetValue(kindCode, out var groupFindings))
            {
                continue;
            }

            groupFindings.Sort(CompareFindings);
            groups.Add(new ContractCheckReportGroupModel
            {
                KindCode = kindCode,
                FindingCount = groupFindings.Count,
                Findings = groupFindings
            });
        }

        return groups;
    }

    // Ayni kind grubundaki bulgulari adres, severity, direction ve degerlerine gore kararli siralar.
    private static int CompareFindings(Finding? left, Finding? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        if (right is null)
        {
            return 1;
        }

        var result = CompareAddresses(left.Address, right.Address);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(left.SeverityCode, right.SeverityCode);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(left.DirectionCode, right.DirectionCode);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(left.OldValue, right.OldValue);
        return result != 0
            ? result
            : StringComparer.Ordinal.Compare(left.NewValue, right.NewValue);
    }

    // Finding adresinin tum bilesenlerini sabit alan sirasinda karsilastirir.
    private static int CompareAddresses(FindingAddress left, FindingAddress right)
    {
        var leftValues = new[]
        {
            left.OperationId,
            left.HttpMethod,
            left.Path,
            left.SchemaName,
            left.PropertyPath,
            left.ParameterName,
            left.ResponseStatus,
            left.MediaType
        };
        var rightValues = new[]
        {
            right.OperationId,
            right.HttpMethod,
            right.Path,
            right.SchemaName,
            right.PropertyPath,
            right.ParameterName,
            right.ResponseStatus,
            right.MediaType
        };

        for (var index = 0; index < leftValues.Length; index++)
        {
            var result = StringComparer.Ordinal.Compare(leftValues[index], rightValues[index]);
            if (result != 0)
            {
                return result;
            }
        }

        return 0;
    }

    // Eksik kodu sifir kabul ederek ortak count okumasini yapar.
    private static int GetCount(IReadOnlyDictionary<string, int> counts, string code)
    {
        return counts.TryGetValue(code, out var count) ? count : 0;
    }
}
