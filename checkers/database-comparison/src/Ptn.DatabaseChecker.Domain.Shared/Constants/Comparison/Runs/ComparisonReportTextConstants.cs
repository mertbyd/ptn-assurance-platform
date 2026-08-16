namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: HTML/Markdown raporlarinda ortak kullanilan baslik, etiket ve format parcalarini merkezi tutar.
// sistemdeki gorevi: Report generator icinde insan-okur domain metinlerinin raw string olarak dagilmasini engeller.
public static class ComparisonReportTextConstants
{
    // Markdown ana basligi.
    public const string MarkdownTitle = "# Database comparison report";
    // Markdown ozet basligi.
    public const string MarkdownSummaryTitle = "## Summary";
    // Markdown schema bulgu basligi.
    public const string MarkdownSchemaTitle = "## Schema findings";
    // Markdown migration bulgu basligi.
    public const string MarkdownMigrationTitle = "## Migration findings";
    // Markdown data bulgu basligi.
    public const string MarkdownDataTitle = "## Data findings";
    // Tum farklar etiket metni.
    public const string TotalLabel = "Total differences";
    // Schema fark etiketi.
    public const string SchemaLabel = "Schema";
    // Migration fark etiketi.
    public const string MigrationLabel = "Migration";
    // Data fark etiketi.
    public const string DataLabel = "Data";

    // --- HTML ic rapor parcalari (email kabugu icine gomulur, kendi <style> tanimlamaz; inline-style kullanir) ---

    // HTML dokuman acilis etiketi.
    public const string HtmlDocumentStart = "<html><body style=\"font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #1e293b; line-height: 1.5;\">";
    // HTML dokuman kapanis etiketi.
    public const string HtmlDocumentEnd = "</body></html>";
    // HTML ana baslik markup'i.
    public const string HtmlTitle = "<h2 style=\"font-size: 18px; font-weight: 600; color: #0f172a; margin: 0 0 20px 0; padding-bottom: 12px; border-bottom: 2px solid #e2e8f0;\">Comparison Results</h2>";
    // HTML ozet basligi markup'i.
    public const string HtmlSummaryTitle = "";

    // Ozet sayac kartlari icin acilis wrapper; duz bir tablo ile yan yana sayaclar.
    public const string HtmlSummaryCardsStart = @"<table style=""width: 100%; border-collapse: collapse; margin-bottom: 24px;""><tr>";
    public const string HtmlSummaryCardsEnd = @"</tr></table>";

    // Tek bir sayac karti olusturur; format: BuildSummaryCard(label, count, color) ile doldurulur.
    // {0}=background-color, {1}=text-color, {2}=count, {3}=label
    public const string HtmlSummaryCardTemplate = @"<td style=""padding: 4px;""><div style=""background-color: {0}; border-radius: 8px; padding: 14px 16px; text-align: center;""><div style=""font-size: 22px; font-weight: 700; color: {1};"">{2}</div><div style=""font-size: 11px; font-weight: 500; color: {1}; opacity: 0.75; margin-top: 2px; text-transform: uppercase; letter-spacing: 0.05em;"">{3}</div></div></td>";

    // Sayac kart renkleri.
    public const string SummaryTotalBg = "#0f172a";
    public const string SummaryTotalFg = "#ffffff";
    public const string SummarySchemaBg = "#e0f2fe";
    public const string SummarySchemaFg = "#0369a1";
    public const string SummaryMigrationBg = "#fef3c7";
    public const string SummaryMigrationFg = "#92400e";
    public const string SummaryDataBg = "#ccfbf1";
    public const string SummaryDataFg = "#115e59";

    // HTML section basliklari (schema / migration / data).
    public const string HtmlSchemaTitle = @"<h3 style=""font-size: 14px; font-weight: 600; color: #0369a1; margin: 28px 0 12px 0; padding: 6px 12px; background-color: #f0f9ff; border-left: 3px solid #0ea5e9; border-radius: 0 6px 6px 0;"">Schema Findings</h3>";
    public const string HtmlMigrationTitle = @"<h3 style=""font-size: 14px; font-weight: 600; color: #92400e; margin: 28px 0 12px 0; padding: 6px 12px; background-color: #fffbeb; border-left: 3px solid #f59e0b; border-radius: 0 6px 6px 0;"">Migration Findings</h3>";
    public const string HtmlDataTitle = @"<h3 style=""font-size: 14px; font-weight: 600; color: #115e59; margin: 28px 0 12px 0; padding: 6px 12px; background-color: #f0fdfa; border-left: 3px solid #14b8a6; border-radius: 0 6px 6px 0;"">Data Findings</h3>";

    // Finding tablo acilis; her section kendi tablosuna sahiptir.
    public const string HtmlFindingTableStart = @"<table style=""width: 100%; border-collapse: collapse; font-size: 13px;"">";
    public const string HtmlFindingTableEnd = @"</table>";

    // Tek bir finding satirini olusturur. Alternating row renklendirmesi generator tarafinda satir indexine gore secilir.
    // {0}=background-color (#ffffff veya #f8fafc), {1}=encoded finding metni
    public const string HtmlFindingRowTemplate = @"<tr><td style=""padding: 8px 12px; border-bottom: 1px solid #f1f5f9; color: #334155; background-color: {0};"">{1}</td></tr>";
    public const string HtmlFindingRowEvenBg = "#ffffff";
    public const string HtmlFindingRowOddBg = "#f8fafc";

    // Kind badge'i: OnlyInSource=kirmizi, OnlyInTarget=mavi, Modified=amber.
    // {0}=background, {1}=text-color, {2}=encoded kind text
    public const string HtmlKindBadgeTemplate = @"<span style=""display: inline-block; font-size: 11px; font-weight: 600; padding: 2px 8px; border-radius: 4px; background-color: {0}; color: {1}; margin-left: 6px;"">{2}</span>";

    public const string KindOnlyInSourceBg = "#fee2e2";
    public const string KindOnlyInSourceFg = "#991b1b";
    public const string KindOnlyInTargetBg = "#dbeafe";
    public const string KindOnlyInTargetFg = "#1e40af";
    public const string KindModifiedBg = "#fef3c7";
    public const string KindModifiedFg = "#92400e";
    public const string KindDefaultBg = "#f1f5f9";
    public const string KindDefaultFg = "#475569";

    // Object type badge; schema findings icin nesne turunu gosterir.
    // {0}=encoded object type
    public const string HtmlObjectTypeBadgeTemplate = @"<span style=""display: inline-block; font-size: 10px; font-weight: 500; padding: 1px 6px; border-radius: 3px; background-color: #f1f5f9; color: #64748b; margin-left: 4px;"">{0}</span>";

    // "No findings" mesaji.
    public const string HtmlNoFindings = @"<p style=""font-size: 13px; color: #94a3b8; font-style: italic; margin: 8px 0;"">No differences found.</p>";

    // --- Zenginlestirilmis rapor (T12): teknik kod yerine is-anlami + degisen alan + kaynak->hedef kanit ---

    // Fark yonunun insan-okur aciklamasi.
    public const string KindOnlyInSourceText = "Only in source — missing in target";
    public const string KindOnlyInTargetText = "Only in target — added in target";
    public const string KindModifiedText = "Changed";
    public const string KindDefaultText = "Difference";

    // Nesne adresini kalin gosterir. {0}=encoded address
    public const string HtmlAddressTemplate = @"<span style=""font-weight: 600; color: #0f172a;"">{0}</span>";
    // Adresin altindaki ince detay satiri. {0}=inner html
    public const string HtmlDetailLineTemplate = @"<div style=""font-size: 12px; color: #475569; margin-top: 3px;"">{0}</div>";
    // Kaynak/hedef degeri vurgulayan chip. {0}=bg {1}=fg {2}=encoded value
    public const string HtmlValueChipTemplate = @"<code style=""background-color: {0}; color: {1}; padding: 1px 5px; border-radius: 3px; font-size: 11px; font-family: 'SFMono-Regular', Consolas, monospace;"">{2}</code>";
    public const string SourceChipBg = "#fef2f2";
    public const string SourceChipFg = "#991b1b";
    public const string TargetChipBg = "#eff6ff";
    public const string TargetChipFg = "#1e40af";

    // Detay satiri metin parcalari.
    public const string ChangedFieldsLabel = "Fields: ";
    public const string SourceTargetArrow = " &rarr; ";
    public const string EmptyValuePlaceholder = "—";
    public const string RowsLabel = "Rows: ";
    public const string RowsDeltaOpen = " (Δ ";
    public const string RowsDeltaClose = ")";
    public const string RowsDifferSuffix = " row(s) differ";
    public const string CountUnavailable = "n/a";
    // Migration history tablosunun kaynak ve hedef semalarini tanitan etiket.
    public const string MigrationSchemaLabel = "Schema: ";
    public const string MigrationVersionLabel = "EF version: ";
    public const string CellChangeSeparator = ", ";
    public const string RowKeyPrefix = "row ";
    public const string MoreRowsPrefix = "…and ";
    public const string MoreRowsSuffix = " more row(s)";
    // Bir Modified satirda gosterilecek azami ornek satir sayisi (rapor sismesin).
    public const int MaxSampleRowsPerTable = 5;
    // Kaynak/hedef tanim kaniti icin azami uzunluk.
    public const int MaxDefinitionLength = 80;

    // Object-type dagilim basligi + chip'i.
    public const string HtmlBreakdownTitle = @"<h3 style=""font-size: 13px; font-weight: 600; color: #334155; margin: 24px 0 10px 0;"">Breakdown by object type</h3>";
    public const string HtmlBreakdownWrapStart = @"<div style=""margin-bottom: 8px;"">";
    public const string HtmlBreakdownWrapEnd = @"</div>";
    // {0}=encoded type {1}=count
    public const string HtmlBreakdownChipTemplate = @"<span style=""display: inline-block; font-size: 12px; padding: 3px 9px; border-radius: 12px; background-color: #f1f5f9; color: #334155; margin: 0 6px 6px 0;"">{0}: <strong>{1}</strong></span>";

    // Markdown zengin detay parcalari.
    public const string MarkdownChangedFields = " — Fields: ";
    public const string MarkdownArrow = " -> ";
    // Markdown migration satirinda kaynak ve hedef semalarini tanitan parca.
    public const string MarkdownMigrationSchema = " — Schema: ";
    public const string MarkdownRows = " — rows ";

    // Eski uyumluluk icin korunan alanlar (Markdown ve legacy referanslar).
    public const string HtmlListStart = "<ul style=\"margin: 0; padding-left: 20px;\">";
    public const string HtmlListEnd = "</ul>";
    public const string HtmlListItemStart = "<li style=\"padding: 2px 0; font-size: 13px; color: #334155;\">";
    public const string HtmlListItemEnd = "</li>";

    // Adres parcalarini ayiran separator.
    public const string AddressSeparator = ".";
    // Label ve deger arasindaki separator.
    public const string ValueSeparator = ": ";
    // Markdown bullet prefix.
    public const string MarkdownBullet = "- ";
    // Markdown object type acilis separator'i.
    public const string MarkdownObjectTypeStart = " [";
    // Markdown object type kapanis separator'i.
    public const string MarkdownObjectTypeEnd = "] ";
    // Markdown kind code oncesi separator.
    public const string MarkdownKindSeparator = " ";

    // Rapor detay parcalarini ayiran orta nokta.
    public const string DetailSeparator = " · ";

    // Satir sayisi detaylarini ayiran token.
    public const string RowsCountSeparator = "; ";

    // Hucre adi ile degerini ayiran token.
    public const string CellNameValueSeparator = " ";

    // Kesilmis metin gostergesi.
    public const string Ellipsis = "…";

    // Markdown kanit metni acilis token'i.
    public const string MarkdownEvidenceOpen = " (";

    // Markdown kanit metni kapanis token'i.
    public const string MarkdownEvidenceClose = ")";
}
