namespace Ptn.DatabaseChecker.Dtos.Findings;

// islevi: Sema, veri veya migration bulgusunun kucuk-parca okuma icin ortak API temsilidir.
// sistemdeki gorevi: MCP'nin agir run detayini almadan filtreli atomik bulgulari tuketmesini saglar.
/// <summary>
/// Sema, veri veya migration bulgusunun ortak sayfali API modelidir.
/// </summary>
public class FindingDto
{
    /// <summary>Bulgunun kararli SHA-256 kimligi.</summary>
    public string? Fingerprint { get; set; }
    /// <summary>Fingerprint girdisindeki motor ve hedef adres bilesenleri.</summary>
    public FindingAddressDto Address { get; set; } = new();
    /// <summary>Bulgunun uyumluluk siddeti.</summary>
    public string SeverityCode { get; set; } = default!;
    /// <summary>Bulgunun fark yonu.</summary>
    public string KindCode { get; set; } = default!;
    /// <summary>Bulgunun sema nesne turu.</summary>
    public string ObjectTypeCode { get; set; } = default!;
    /// <summary>Bulgunun opsiyonel sema adi.</summary>
    public string? SchemaName { get; set; }
    /// <summary>Bulgunun nesne adi.</summary>
    public string ObjectName { get; set; } = default!;
    /// <summary>Veri bulgulari icin tablo adi.</summary>
    public string? TableName { get; set; }
    /// <summary>Kolon gibi opsiyonel alt nesne adi.</summary>
    public string? ChildName { get; set; }
    /// <summary>Varsa karsilastirma guven kodu.</summary>
    public string? ConfidenceCode { get; set; }
    /// <summary>Kaynak taraf kanit degeri.</summary>
    public string? SourceValue { get; set; }
    /// <summary>Hedef taraf kanit degeri.</summary>
    public string? TargetValue { get; set; }
    /// <summary>Kaynak taraf satir sayisi.</summary>
    public long? SourceRowCount { get; set; }
    /// <summary>Hedef taraf satir sayisi.</summary>
    public long? TargetRowCount { get; set; }
    /// <summary>Kaynak eksi hedef satir sayisi.</summary>
    public long? RowCountDifference { get; set; }
    /// <summary>Degisen alanlarin opsiyonel ozeti.</summary>
    public string? ChangeSummary { get; set; }
}
