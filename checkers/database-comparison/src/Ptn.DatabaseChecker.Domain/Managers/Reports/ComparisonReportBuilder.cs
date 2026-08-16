using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Comparison.Reports;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Reports;

// islevi: Bir run'in owned bulgularindan raporun aggregation kismini uretir: ozet sayaclari + tur/tablo bazli gruplama.
// sistemdeki gorevi: T8 raporunun aggregation uretim noktasi; saf domain servisidir (I/O yok). Header/detay ESLEMEZ (onu Mapperly entity->DTO yapar); finding'leri de yeniden modellemez - yalnizca Mapperly'nin yapamadigi sayim/gruplama isini yapar. Motor okuma/karsilastirma MigrationComparisonManager/SchemaComparisonManager'da kalir.
public class ComparisonReportBuilder : ITransientDependency
{
    // islevi: Bulgulardan yalnizca aggregation (ozet + tur/tablo gruplari) hesaplar; header/detay disaridan (Mapperly) gelir.
    public ComparisonReportAggregationModel Build(ComparisonFindings findings)
    {
        var entries = BuildEntries(findings);

        return new ComparisonReportAggregationModel
        {
            Summary = BuildSummary(findings, entries),
            ObjectTypeGroups = BuildGroups(entries, entry => entry.ObjectTypeCode),
            TableGroups = BuildGroups(entries, entry => entry.TableKey)
        };
    }

    // islevi: Uc bulgu ailesini (sema/migration/veri) yalnizca gruplama/sayim icin gereken (turkodu, tabloanahtari, yon) ucdegerine indirger.
    // Bu efemeral projeksiyondur; finding'in tam kopyasi DEGILDIR (detay Findings'te kalir), sadece cross-tur sayim icin anahtar cikarir.
    private static List<ReportEntry> BuildEntries(ComparisonFindings findings)
    {
        var entries = new List<ReportEntry>();

        entries.AddRange(findings.SchemaDifferences.Select(difference => new ReportEntry(
            difference.ObjectTypeCode,
            BuildTableKey(difference.SchemaName, difference.ObjectName),
            difference.KindCode)));

        entries.AddRange(findings.MigrationDifferences.Select(difference => new ReportEntry(
            ComparisonReportCategoryCodes.Migration,
            difference.MigrationId,
            difference.KindCode)));

        entries.AddRange(findings.DataDifferences.Select(difference => new ReportEntry(
            ComparisonReportCategoryCodes.Data,
            BuildTableKey(difference.SchemaName, difference.TableName),
            difference.KindCode)));

        return entries;
    }

    // islevi: Ozet blogunu (toplam + kategori sayaclari + yon/nesne-turu dagilimi) bulgulardan hesaplar.
    private static ComparisonReportSummaryModel BuildSummary(ComparisonFindings findings, List<ReportEntry> entries)
        => new()
        {
            TotalDifferenceCount = entries.Count,
            SchemaDifferenceCount = findings.SchemaDifferences.Count,
            DataDifferenceCount = findings.DataDifferences.Count,
            MigrationDifferenceCount = findings.MigrationDifferences.Count,
            KindCounts = BuildCounts(entries, entry => entry.KindCode),
            ObjectTypeCounts = BuildCounts(entries, entry => entry.ObjectTypeCode)
        };

    // islevi: Kayitlari verilen anahtara gore (nesne turu ya da tablo) sirali gruplara ceviren tek gruplama noktasi (is bir kez yapilir).
    private static List<ComparisonReportGroupModel> BuildGroups(
        List<ReportEntry> entries,
        Func<ReportEntry, string> keySelector)
        => entries
            .GroupBy(keySelector, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ComparisonReportGroupModel
            {
                GroupKey = group.Key,
                DifferenceCount = group.Count(),
                KindCounts = BuildCounts(group, entry => entry.KindCode)
            })
            .ToList();

    // islevi: Verilen anahtara gore fark sayaclarini (buyukten kucuge) hesaplar; hem ozet dagilimi hem grup ici yon dagilimi bunu kullanir.
    private static List<ComparisonReportCountModel> BuildCounts(
        IEnumerable<ReportEntry> entries,
        Func<ReportEntry, string> keySelector)
        => entries
            .GroupBy(keySelector, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ComparisonReportCountModel { Code = group.Key, Count = group.Count() })
            .OrderByDescending(count => count.Count)
            .ThenBy(count => count.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();

    // islevi: Sema+nesne kimligini tablo bazli gruplama anahtarina (sema.nesne) cevirir.
    private static string BuildTableKey(string schemaName, string objectName)
        => string.IsNullOrEmpty(schemaName) ? objectName : $"{schemaName}.{objectName}";

    // islevi: Gruplama/sayim icin gereken minimal anahtar ucdegeri; disari sizmayan efemeral projeksiyon.
    private readonly record struct ReportEntry(string ObjectTypeCode, string TableKey, string KindCode);
}
