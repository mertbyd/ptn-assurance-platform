namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

// islevi: PostgreSQL pg_attrdef katalog satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Kolon default expression'larini pg_get_expr ile raporlanabilir SQL metnine cevirmek icin ham parse tree bilgisini tasir.
public sealed class PostgreSqlAttributeDefaultCatalogRow
{
    // pg_attrdef.adrelid: default'un ait oldugu tablo kimligi.
    public uint RelationId { get; set; }

    // pg_attrdef.adnum: default'un ait oldugu kolon numarasi.
    public short ColumnNumber { get; set; }

    // pg_attrdef.adbin: default expression internal parse tree degeri.
    public string BinaryExpression { get; set; } = string.Empty;
}
