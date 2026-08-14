using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Bir tablonun assertion yazarligi icin gerekli kolon ve anahtar ozetini tasir.
// sistemdeki gorevi: Database Checker sema DTO'sunu Domain icinde provider-bagimsiz veri kabuguna cevirir.
public sealed class PtnTableDescription
{
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<PtnTableColumn> Columns { get; set; } = [];
    public PtnTableKey? PrimaryKey { get; set; }
    public List<PtnTableKey> UniqueIndexes { get; set; } = [];
}
