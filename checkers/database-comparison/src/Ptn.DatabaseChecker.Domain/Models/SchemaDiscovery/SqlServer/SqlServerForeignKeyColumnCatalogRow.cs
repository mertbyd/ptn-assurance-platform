namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys.foreign_key_columns katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: FK kisitlamalarinin hangi kolonlardan hangi kolonlara baglangic yaptigini tasiyan salt-okunur katalog modelidir.
public sealed class SqlServerForeignKeyColumnCatalogRow
{
    // sys.foreign_key_columns.constraint_object_id: FK kisitlama kimligi.
    public int ConstraintObjectId { get; set; }

    // sys.foreign_key_columns.constraint_column_id: FK kolon sira numarasi.
    public int ConstraintColumnId { get; set; }

    // sys.foreign_key_columns.parent_object_id: kaynak tablo kimligi.
    public int ParentObjectId { get; set; }

    // sys.foreign_key_columns.parent_column_id: kaynak kolon kimligi.
    public int ParentColumnId { get; set; }

    // sys.foreign_key_columns.referenced_object_id: hedef tablo kimligi.
    public int ReferencedObjectId { get; set; }

    // sys.foreign_key_columns.referenced_column_id: hedef kolon kimligi.
    public int ReferencedColumnId { get; set; }
}
