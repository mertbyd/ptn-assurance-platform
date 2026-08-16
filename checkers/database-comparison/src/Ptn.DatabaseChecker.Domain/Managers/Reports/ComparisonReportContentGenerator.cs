using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Comparison.Reports;
using Volo.Abp;
using Text = Ptn.DatabaseChecker.Constants.Comparison.ComparisonReportTextConstants;

namespace Ptn.DatabaseChecker.Managers.Reports;

// islevi: ComparisonFindings modelinden Html ve Markdown rapor icerigi uretir.
// sistemdeki gorevi: Report formatlama ve persisted-content fallback kararini AppService/controller'dan ayirir; email gondermez. Her bulguyu yalniz teknik kodla degil is-anlamiyla (ne degisti, kaynak->hedef, kac satir) detaylandirir.
public class ComparisonReportContentGenerator : DatabaseCheckerDomainService
{
    // islevi: Findings'ten her iki desteklenen rapor formatini uretir.
    public List<ComparisonReportContent> Generate(ComparisonFindings findings)
        => new()
        {
            new ComparisonReportContent
            {
                FormatCode = ReportFormatCodes.Html,
                Content = BuildHtml(findings)
            },
            new ComparisonReportContent
            {
                FormatCode = ReportFormatCodes.Markdown,
                Content = BuildMarkdown(findings)
            }
        };

    // islevi: Persisted rapor varsa onu, yoksa mevcut findings'ten uretilen icerigi format filtresiyle secer.
    public List<ComparisonReportContent> Select(
        ComparisonFindings findings,
        List<ComparisonReportContent> persistedContents,
        string? requestedFormat)
    {
        var format = string.IsNullOrWhiteSpace(requestedFormat)
            ? null
            : requestedFormat;
        if (format is not null && !ReportFormatCodes.IsSupported(format))
        {
            throw new BusinessException(ComparisonRunExceptionCodes.InvalidReportFormat);
        }

        var contents = persistedContents.Count == 0 ? Generate(findings) : persistedContents;
        if (format is null)
        {
            return contents;
        }

        var selected = contents.Where(x => x.FormatCode == format).ToList();
        return selected.Count > 0
            ? selected
            : Generate(findings).Where(x => x.FormatCode == format).ToList();
    }

    // islevi: Summary + tur dagilimi + schema/migration/data satirlarini styled HTML metnine cevirir.
    private static string BuildHtml(ComparisonFindings findings)
    {
        var builder = new StringBuilder(Text.HtmlDocumentStart);
        builder.Append(Text.HtmlTitle);
        AppendHtmlSummaryCards(builder, findings);
        AppendHtmlBreakdown(builder, findings);
        AppendHtmlSchema(builder, findings);
        AppendHtmlMigrations(builder, findings);
        AppendHtmlData(builder, findings);
        builder.Append(Text.HtmlDocumentEnd);
        return builder.ToString();
    }

    // islevi: Summary satirlarini renkli sayac kartlari olarak ekler.
    private static void AppendHtmlSummaryCards(StringBuilder builder, ComparisonFindings findings)
    {
        builder.Append(Text.HtmlSummaryCardsStart);
        AppendSummaryCard(builder, Text.TotalLabel, TotalCount(findings), Text.SummaryTotalBg, Text.SummaryTotalFg);
        AppendSummaryCard(builder, Text.SchemaLabel, findings.SchemaDifferences.Count, Text.SummarySchemaBg, Text.SummarySchemaFg);
        AppendSummaryCard(builder, Text.MigrationLabel, findings.MigrationDifferences.Count, Text.SummaryMigrationBg, Text.SummaryMigrationFg);
        AppendSummaryCard(builder, Text.DataLabel, findings.DataDifferences.Count, Text.SummaryDataBg, Text.SummaryDataFg);
        builder.Append(Text.HtmlSummaryCardsEnd);
    }

    // islevi: Tek bir sayac kartini render eder.
    private static void AppendSummaryCard(StringBuilder builder, string label, int count, string bg, string fg)
        => builder.AppendFormat(Text.HtmlSummaryCardTemplate, bg, fg, count, WebUtility.HtmlEncode(label));

    // islevi: Schema farklarini nesne turu bazinda sayan bir hizli dagilim seridi ekler (rapor bir bakista okunur olsun).
    private static void AppendHtmlBreakdown(StringBuilder builder, ComparisonFindings findings)
    {
        if (findings.SchemaDifferences.Count == 0)
        {
            return;
        }

        builder.Append(Text.HtmlBreakdownTitle).Append(Text.HtmlBreakdownWrapStart);
        foreach (var group in findings.SchemaDifferences
                     .GroupBy(difference => difference.ObjectTypeCode, StringComparer.OrdinalIgnoreCase)
                     .OrderByDescending(group => group.Count())
                     .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendFormat(Text.HtmlBreakdownChipTemplate, WebUtility.HtmlEncode(group.Key), group.Count());
        }

        builder.Append(Text.HtmlBreakdownWrapEnd);
    }

    // islevi: HTML schema finding listesini adres + tur + yon + "ne degisti" + kaynak->hedef kanit ile styled tablo olarak ekler.
    private static void AppendHtmlSchema(StringBuilder builder, ComparisonFindings findings)
    {
        builder.Append(Text.HtmlSchemaTitle);
        AppendHtmlRows(builder, findings.SchemaDifferences, BuildSchemaRowContent);
    }

    // islevi: HTML migration finding listesini surum kaymasi detayiyla styled tablo olarak ekler.
    private static void AppendHtmlMigrations(StringBuilder builder, ComparisonFindings findings)
    {
        builder.Append(Text.HtmlMigrationTitle);
        AppendHtmlRows(builder, findings.MigrationDifferences, BuildMigrationRowContent);
    }

    // islevi: HTML data finding listesini satir sayisi + degisen hucre ornekleriyle styled tablo olarak ekler.
    private static void AppendHtmlData(StringBuilder builder, ComparisonFindings findings)
    {
        builder.Append(Text.HtmlDataTitle);
        AppendHtmlRows(builder, findings.DataDifferences, BuildDataRowContent);
    }

    // islevi: Bir finding koleksiyonunu, verilen icerik ureticisiyle alternating-row tablosuna cevirir (bos ise "fark yok").
    private static void AppendHtmlRows<TFinding>(
        StringBuilder builder,
        List<TFinding> findings,
        Func<TFinding, string> rowContentSelector)
    {
        if (findings.Count == 0)
        {
            builder.Append(Text.HtmlNoFindings);
            return;
        }

        builder.Append(Text.HtmlFindingTableStart);
        for (var index = 0; index < findings.Count; index++)
        {
            var rowBackground = index % 2 == 0 ? Text.HtmlFindingRowEvenBg : Text.HtmlFindingRowOddBg;
            builder.AppendFormat(Text.HtmlFindingRowTemplate, rowBackground, rowContentSelector(findings[index]));
        }
        builder.Append(Text.HtmlFindingTableEnd);
    }

    // islevi: Tek schema farkinin zengin HTML icerigini uretir (adres, turler, yon aciklamasi, degisen alanlar, kaynak->hedef tanim).
    private static string BuildSchemaRowContent(SchemaDifferenceModel difference)
    {
        var address = string.Format(Text.HtmlAddressTemplate, WebUtility.HtmlEncode(BuildAddress(difference.SchemaName, difference.ObjectName, difference.ChildName)));
        var objectTypeBadge = string.Format(Text.HtmlObjectTypeBadgeTemplate, WebUtility.HtmlEncode(difference.ObjectTypeCode));
        var kindBadge = BuildKindBadge(difference.KindCode);

        var detail = new StringBuilder(WebUtility.HtmlEncode(KindText(difference.KindCode)));
        if (!string.IsNullOrWhiteSpace(difference.ChangeSummary))
        {
            detail.Append(Text.DetailSeparator).Append(Text.ChangedFieldsLabel).Append(WebUtility.HtmlEncode(difference.ChangeSummary));
        }

        var content = new StringBuilder($"{address}{objectTypeBadge}{kindBadge}");
        content.AppendFormat(Text.HtmlDetailLineTemplate, detail.ToString());
        var evidence = BuildSourceTargetEvidence(difference.SourceDefinition, difference.TargetDefinition);
        if (evidence is not null)
        {
            content.AppendFormat(Text.HtmlDetailLineTemplate, evidence);
        }
        return content.ToString();
    }

    // islevi: Tek migration farkinin zengin HTML icerigini uretir (id, yon, kaynak/hedef semasi, EF surum kaymasi).
    private static string BuildMigrationRowContent(MigrationDifferenceModel difference)
    {
        var id = string.Format(Text.HtmlAddressTemplate, WebUtility.HtmlEncode(difference.MigrationId));
        var kindBadge = BuildKindBadge(difference.KindCode);
        var detail = new StringBuilder(WebUtility.HtmlEncode(KindText(difference.KindCode)));
        if (difference.SourceSchemaName is not null || difference.TargetSchemaName is not null)
        {
            detail.Append(Text.DetailSeparator).Append(Text.MigrationSchemaLabel)
                .Append(BuildSourceTargetEvidence(difference.SourceSchemaName, difference.TargetSchemaName));
        }
        if (difference.SourceProductVersion is not null || difference.TargetProductVersion is not null)
        {
            detail.Append(Text.DetailSeparator).Append(Text.MigrationVersionLabel)
                .Append(BuildSourceTargetEvidence(difference.SourceProductVersion, difference.TargetProductVersion));
        }

        var content = new StringBuilder($"{id}{kindBadge}");
        content.AppendFormat(Text.HtmlDetailLineTemplate, detail.ToString());
        return content.ToString();
    }

    // islevi: Tek data farkinin zengin HTML icerigini uretir (adres, yon, satir sayisi, degisen hucre ornekleri).
    private static string BuildDataRowContent(DataDifferenceModel difference)
    {
        var address = string.Format(Text.HtmlAddressTemplate, WebUtility.HtmlEncode(BuildAddress(difference.SchemaName, difference.TableName, null)));
        var kindBadge = BuildKindBadge(difference.KindCode);

        var detail = new StringBuilder(WebUtility.HtmlEncode(KindText(difference.KindCode)));
        detail.Append(Text.DetailSeparator).Append(Text.RowsLabel)
            .Append(FormatCount(difference.SourceRowCount)).Append(Text.SourceTargetArrow).Append(FormatCount(difference.TargetRowCount));
        if (difference.RowCountDifference.HasValue)
        {
            detail.Append(Text.RowsDeltaOpen).Append(difference.RowCountDifference.Value).Append(Text.RowsDeltaClose);
        }
        if (difference.RowDifferences.Count > 0)
        {
            detail.Append(Text.RowsCountSeparator).Append(difference.RowDifferences.Count).Append(Text.RowsDifferSuffix);
        }

        var content = new StringBuilder($"{address}{kindBadge}");
        content.AppendFormat(Text.HtmlDetailLineTemplate, detail.ToString());

        var shown = 0;
        foreach (var row in difference.RowDifferences)
        {
            if (shown >= Text.MaxSampleRowsPerTable)
            {
                content.AppendFormat(Text.HtmlDetailLineTemplate,
                    WebUtility.HtmlEncode($"{Text.MoreRowsPrefix}{difference.RowDifferences.Count - shown}{Text.MoreRowsSuffix}"));
                break;
            }
            content.AppendFormat(Text.HtmlDetailLineTemplate, BuildRowDetail(row));
            shown++;
        }
        return content.ToString();
    }

    // islevi: Tek bir degisen satirin PK + hucre farklarini (kolon: kaynak->hedef) HTML'e cevirir.
    private static string BuildRowDetail(DataRowDifferenceModel row)
    {
        var builder = new StringBuilder(WebUtility.HtmlEncode($"{Text.RowKeyPrefix}{row.PrimaryKeyValue}: "));
        if (row.ValueDifferences.Count == 0)
        {
            builder.Append(WebUtility.HtmlEncode(KindText(row.KindCode)));
            return builder.ToString();
        }

        builder.Append(string.Join(Text.CellChangeSeparator, row.ValueDifferences.Select(value =>
            WebUtility.HtmlEncode(value.ColumnName) + Text.CellNameValueSeparator
            + BuildValueChip(value.SourceValue, Text.SourceChipBg, Text.SourceChipFg)
            + Text.SourceTargetArrow
            + BuildValueChip(value.TargetValue, Text.TargetChipBg, Text.TargetChipFg))));
        return builder.ToString();
    }

    // islevi: Kaynak/hedef tanim veya deger ciftini renkli chip'lerle "kaynak -> hedef" kanitina cevirir; ikisi de bossa null.
    private static string? BuildSourceTargetEvidence(string? source, string? target)
    {
        if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(target))
        {
            return null;
        }

        return BuildValueChip(source, Text.SourceChipBg, Text.SourceChipFg)
            + Text.SourceTargetArrow
            + BuildValueChip(target, Text.TargetChipBg, Text.TargetChipFg);
    }

    // islevi: Bir degeri (bos ise placeholder, uzunsa kirpilmis) renkli chip HTML'ine cevirir.
    private static string BuildValueChip(string? value, string background, string foreground)
    {
        var text = string.IsNullOrWhiteSpace(value)
            ? Text.EmptyValuePlaceholder
            : Truncate(value, Text.MaxDefinitionLength);
        return string.Format(Text.HtmlValueChipTemplate, background, foreground, WebUtility.HtmlEncode(text));
    }

    // islevi: Uzun tanim metnini rapor sismesin diye kirpar.
    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + Text.Ellipsis;

    // islevi: Fark yon kodunu insan-okur is anlamina cevirir (teknik kod yerine).
    private static string KindText(string kindCode) => kindCode switch
    {
        DifferenceKindCodes.OnlyInSource => Text.KindOnlyInSourceText,
        DifferenceKindCodes.OnlyInTarget => Text.KindOnlyInTargetText,
        DifferenceKindCodes.Modified => Text.KindModifiedText,
        _ => Text.KindDefaultText
    };

    // islevi: Nullable row-count'u insan-okur metne cevirir (null = sayilamadi).
    private static string FormatCount(long? count) => count?.ToString() ?? Text.CountUnavailable;

    // ================= Markdown =================

    // islevi: Summary ve schema/migration/data satirlarini Markdown metnine cevirir.
    private static string BuildMarkdown(ComparisonFindings findings)
    {
        var builder = new StringBuilder();
        builder.AppendLine(Text.MarkdownTitle);
        builder.AppendLine(Text.MarkdownSummaryTitle);
        AppendMarkdownSummary(builder, findings);
        AppendMarkdownSchema(builder, findings);
        AppendMarkdownMigrations(builder, findings);
        AppendMarkdownData(builder, findings);
        return builder.ToString();
    }

    // islevi: Markdown summary satirlarini ekler.
    private static void AppendMarkdownSummary(StringBuilder builder, ComparisonFindings findings)
    {
        builder.AppendLine($"{Text.MarkdownBullet}{Text.TotalLabel}{Text.ValueSeparator}{TotalCount(findings)}");
        builder.AppendLine($"{Text.MarkdownBullet}{Text.SchemaLabel}{Text.ValueSeparator}{findings.SchemaDifferences.Count}");
        builder.AppendLine($"{Text.MarkdownBullet}{Text.MigrationLabel}{Text.ValueSeparator}{findings.MigrationDifferences.Count}");
        builder.AppendLine($"{Text.MarkdownBullet}{Text.DataLabel}{Text.ValueSeparator}{findings.DataDifferences.Count}");
    }

    // islevi: Markdown schema finding listesini "ne degisti" + kaynak->hedef ile ekler.
    private static void AppendMarkdownSchema(StringBuilder builder, ComparisonFindings findings)
    {
        builder.AppendLine(Text.MarkdownSchemaTitle);
        foreach (var difference in findings.SchemaDifferences)
        {
            var line = new StringBuilder($"{Text.MarkdownBullet}{BuildAddress(difference.SchemaName, difference.ObjectName, difference.ChildName)}{Text.MarkdownObjectTypeStart}{difference.ObjectTypeCode}{Text.MarkdownObjectTypeEnd}{KindText(difference.KindCode)}");
            if (!string.IsNullOrWhiteSpace(difference.ChangeSummary))
            {
                line.Append(Text.MarkdownChangedFields).Append(difference.ChangeSummary);
            }
            AppendMarkdownEvidence(line, difference.SourceDefinition, difference.TargetDefinition);
            builder.AppendLine(line.ToString());
        }
    }

    // islevi: Markdown migration finding listesini kaynak/hedef semasi ve surum kaymasiyla ekler.
    private static void AppendMarkdownMigrations(StringBuilder builder, ComparisonFindings findings)
    {
        builder.AppendLine(Text.MarkdownMigrationTitle);
        foreach (var difference in findings.MigrationDifferences)
        {
            var line = new StringBuilder($"{Text.MarkdownBullet}{difference.MigrationId}{Text.MarkdownKindSeparator}{KindText(difference.KindCode)}");
            if (difference.SourceSchemaName is not null || difference.TargetSchemaName is not null)
            {
                line.Append(Text.MarkdownMigrationSchema)
                    .Append(difference.SourceSchemaName ?? Text.EmptyValuePlaceholder)
                    .Append(Text.MarkdownArrow)
                    .Append(difference.TargetSchemaName ?? Text.EmptyValuePlaceholder);
            }
            if (difference.SourceProductVersion is not null || difference.TargetProductVersion is not null)
            {
                line.Append(Text.MarkdownChangedFields).Append(Text.MigrationVersionLabel)
                    .Append(difference.SourceProductVersion ?? Text.EmptyValuePlaceholder)
                    .Append(Text.MarkdownArrow)
                    .Append(difference.TargetProductVersion ?? Text.EmptyValuePlaceholder);
            }
            builder.AppendLine(line.ToString());
        }
    }

    // islevi: Markdown data finding listesini satir sayilariyla ekler.
    private static void AppendMarkdownData(StringBuilder builder, ComparisonFindings findings)
    {
        builder.AppendLine(Text.MarkdownDataTitle);
        foreach (var difference in findings.DataDifferences)
        {
            var line = new StringBuilder($"{Text.MarkdownBullet}{BuildAddress(difference.SchemaName, difference.TableName, null)}{Text.MarkdownKindSeparator}{KindText(difference.KindCode)}");
            line.Append(Text.MarkdownRows).Append(FormatCount(difference.SourceRowCount)).Append(Text.MarkdownArrow).Append(FormatCount(difference.TargetRowCount));
            if (difference.RowDifferences.Count > 0)
            {
                line.Append(Text.RowsCountSeparator).Append(difference.RowDifferences.Count).Append(Text.RowsDifferSuffix);
            }
            builder.AppendLine(line.ToString());
        }
    }

    // islevi: Markdown satirina "(kaynak -> hedef)" kanitini ekler.
    private static void AppendMarkdownEvidence(StringBuilder line, string? source, string? target)
    {
        if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        line.Append(Text.MarkdownEvidenceOpen)
            .Append(string.IsNullOrWhiteSpace(source) ? Text.EmptyValuePlaceholder : Truncate(source, Text.MaxDefinitionLength))
            .Append(Text.MarkdownArrow)
            .Append(string.IsNullOrWhiteSpace(target) ? Text.EmptyValuePlaceholder : Truncate(target, Text.MaxDefinitionLength))
            .Append(Text.MarkdownEvidenceClose);
    }

    // islevi: KindCode degerine gore renklendirilmis badge HTML'i uretir.
    private static string BuildKindBadge(string kindCode)
    {
        var (bg, fg) = ResolveKindColors(kindCode);
        return string.Format(Text.HtmlKindBadgeTemplate, bg, fg, WebUtility.HtmlEncode(kindCode));
    }

    // islevi: Kind koduna gore badge renk ciftini belirler.
    private static (string bg, string fg) ResolveKindColors(string kindCode) => kindCode switch
    {
        DifferenceKindCodes.OnlyInSource => (Text.KindOnlyInSourceBg, Text.KindOnlyInSourceFg),
        DifferenceKindCodes.OnlyInTarget => (Text.KindOnlyInTargetBg, Text.KindOnlyInTargetFg),
        DifferenceKindCodes.Modified => (Text.KindModifiedBg, Text.KindModifiedFg),
        _ => (Text.KindDefaultBg, Text.KindDefaultFg)
    };

    // islevi: Sema/nesne/child adresini raporun tek adres formatina cevirir.
    private static string BuildAddress(string schemaName, string objectName, string? childName)
    {
        var address = string.Join(Text.AddressSeparator, schemaName, objectName);
        return string.IsNullOrWhiteSpace(childName)
            ? address
            : string.Join(Text.AddressSeparator, address, childName);
    }

    // islevi: Tum top-level finding ailelerinin toplam sayisini hesaplar.
    private static int TotalCount(ComparisonFindings findings)
        => findings.SchemaDifferences.Count + findings.MigrationDifferences.Count + findings.DataDifferences.Count;

}
