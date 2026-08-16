namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Bir tablonun kolon ve anahtar ozetini tasir.
// sistemdeki gorevi: Provider-bagimsiz sema sonucunu public Bridge kontratinda sunar.
public sealed class TableDescriptionDto
{
    /// <summary>
    /// Hedef semanin kararli adini belirtir.
    /// </summary>
    public string DbSchemaName { get; set; } = string.Empty;
    /// <summary>
    /// Hedef tablonun adini veya kararli adresini belirtir.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// Isleme katilan kolon adlarini kararli sirada listeler.
    /// </summary>
    public List<TableColumnDto> Columns { get; set; } = [];
    /// <summary>
    /// Tablonun birincil anahtar tanimini tasir.
    /// </summary>
    public TableKeyDto? PrimaryKey { get; set; }
    /// <summary>
    /// Sozlesmenin unique indexes bilgisini belirtir.
    /// </summary>
    public List<TableKeyDto> UniqueIndexes { get; set; } = [];
    /// <summary>
    /// Tablonun gelen ve giden bir seviyeli FK komsularini kararli sirada tasir.
    /// </summary>
    public List<ForeignKeyNeighborDto> ForeignKeyNeighbors { get; set; } = [];
    /// <summary>
    /// Assertion yazarligini etkileyen kararli sema lint uyarilarini tasir.
    /// </summary>
    public List<SchemaLintWarningDto> LintWarnings { get; set; } = [];
    /// <summary>
    /// Ajanin oturum icinde isaret edecegi kapali tablo kimligini tasir.
    /// </summary>
    public Guid TableReferenceId { get; set; }
    /// <summary>
    /// Ajanin assertion yazabilecegi kolonlari kapali kume olarak listeler.
    /// </summary>
    public List<string> AssertableFields { get; set; } = [];
    /// <summary>
    /// Ajanin secebilecegi kapali matcher kodlarini listeler.
    /// </summary>
    public List<string> AllowedMatchers { get; set; } = [];
    /// <summary>
    /// Ajanin anahtar olarak kullanabilecegi kolon gruplarini listeler.
    /// </summary>
    public List<string> KeyCandidates { get; set; } = [];
}
