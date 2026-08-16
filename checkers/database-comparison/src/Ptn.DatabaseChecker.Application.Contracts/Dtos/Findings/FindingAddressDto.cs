namespace Ptn.DatabaseChecker.Dtos.Findings;

// islevi: Database bulgusunun motor cifti ve schema/object/child adres bilesenlerini public cevapta tasir.
// sistemdeki gorevi: Test Module'un bulgu adresini tahmin etmeden FindingAddressGrammar ile ayni fingerprint girdisini kurmasini saglar.
/// <summary>Database bulgusunun kararli ve tipli hedef adresidir.</summary>
public class FindingAddressDto
{
    /// <summary>Kaynak veritabani motor kodu.</summary>
    public string SourceEngineCode { get; set; } = default!;
    /// <summary>Hedef veritabani motor kodu.</summary>
    public string TargetEngineCode { get; set; } = default!;
    /// <summary>Katalog tarafindan cozulmus sema adi; cozulmemisse null.</summary>
    public string? SchemaName { get; set; }
    /// <summary>Table, Column veya Migration gibi sema nesne turu.</summary>
    public string ObjectTypeCode { get; set; } = default!;
    /// <summary>Tablo, migration veya diger ana nesne adi.</summary>
    public string ObjectName { get; set; } = default!;
    /// <summary>Kolon gibi opsiyonel alt nesne adi.</summary>
    public string? ChildName { get; set; }
}
