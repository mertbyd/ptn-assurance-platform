using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Bir tablonun assertion yazarligi icin gerekli kolon ve anahtar ozetini tasir.
// sistemdeki gorevi: Database Checker sema DTO'sunu Domain icinde provider-bagimsiz veri kabuguna cevirir.
public sealed class TableDescription
{
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<TableColumn> Columns { get; set; } = [];
    public TableKey? PrimaryKey { get; set; }
    public List<TableKey> UniqueIndexes { get; set; } = [];
    public List<ForeignKeyNeighbor> ForeignKeyNeighbors { get; set; } = [];
    public List<SchemaLintWarning> LintWarnings { get; set; } = [];
}
