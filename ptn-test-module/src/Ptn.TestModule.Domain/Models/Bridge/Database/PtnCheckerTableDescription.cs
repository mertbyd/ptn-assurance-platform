using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Database checker tablo taniminin kaynak alan seklini tasir.
// sistemdeki gorevi: SchemaName semantigini Manager'a birakip Mapperly eslemesini attribute'suz tutar.
public sealed class PtnCheckerTableDescription
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<PtnTableColumn> Columns { get; set; } = [];
    public PtnTableKey? PrimaryKey { get; set; }
    public List<PtnTableKey> UniqueIndexes { get; set; } = [];
}
