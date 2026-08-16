namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Desteklenen motorlarin kanonik tipe eslenebilen ham katalog tip adlarini tek kaynakta sabitler.
// sistemdeki gorevi: EF tip-esleme tablolarinin provider string'lerini daginik ve magic literal olarak tasimasini engeller.
public static class EngineDataTypeNameCodes
{
    // islevi: PostgreSQL pg_type.typname degerlerinin desteklenen alt kumesini sabitler.
    // sistemdeki gorevi: PostgreSqlEngineTypeMapProvider tablosunun provider-katalog sozlesmesini tasir.
    public static class PostgreSql
    {
        public const string VarChar = DatabaseMetadataCatalogConstants.PostgreSql.VarCharTypeName;
        public const string CharacterVarying = "character varying";
        public const string BpChar = DatabaseMetadataCatalogConstants.PostgreSql.CharTypeName;
        public const string Character = "character";
        public const string Char = "char";
        public const string Name = "name";
        public const string Citext = "citext";
        public const string Text = "text";
        public const string SmallInt = "int2";
        public const string SmallInteger = "smallint";
        public const string Integer = "int4";
        public const string IntegerAlias = "integer";
        public const string BigInt = "int8";
        public const string BigInteger = "bigint";
        public const string Numeric = DatabaseMetadataCatalogConstants.PostgreSql.NumericTypeName;
        public const string Decimal = "decimal";
        public const string Real = "float4";
        public const string RealAlias = "real";
        public const string DoublePrecision = "float8";
        public const string DoublePrecisionAlias = "double precision";
        public const string Boolean = "bool";
        public const string BooleanAlias = "boolean";
        public const string Date = "date";
        public const string Time = "time";
        public const string TimeWithoutTimeZone = "time without time zone";
        public const string TimeWithTimeZone = "timetz";
        public const string TimeWithTimeZoneAlias = "time with time zone";
        public const string Timestamp = "timestamp";
        public const string TimestampWithoutTimeZone = "timestamp without time zone";
        public const string TimestampWithTimeZone = "timestamptz";
        public const string TimestampWithTimeZoneAlias = "timestamp with time zone";
        public const string Interval = "interval";
        public const string Uuid = "uuid";
        public const string Bytea = "bytea";
        public const string Bit = "bit";
        public const string VarBit = "varbit";
        public const string Json = "json";
        public const string JsonBinary = "jsonb";
        public const string JsonPath = "jsonpath";
        public const string Xml = "xml";
        public const string Money = "money";
        public const string Inet = "inet";
        public const string Cidr = "cidr";
        public const string MacAddress = "macaddr";
        public const string MacAddress8 = "macaddr8";
        public const string Point = "point";
        public const string Line = "line";
        public const string LineSegment = "lseg";
        public const string Box = "box";
        public const string Path = "path";
        public const string Polygon = "polygon";
        public const string Circle = "circle";
        public const string Geometry = "geometry";
        public const string Geography = "geography";
        public const string Unknown = "unknown";
        public const string TsVector = "tsvector";
        public const string BooleanArray = "_bool";
        public const string SmallIntegerArray = "_int2";
        public const string IntegerArray = "_int4";
        public const string BigIntegerArray = "_int8";
        public const string NumericArray = "_numeric";
        public const string RealArray = "_float4";
        public const string DoubleArray = "_float8";
        public const string TextArray = "_text";
        public const string VarCharArray = "_varchar";
        public const string BpCharArray = "_bpchar";
        public const string DateArray = "_date";
        public const string TimeArray = "_time";
        public const string TimeWithTimeZoneArray = "_timetz";
        public const string TimestampArray = "_timestamp";
        public const string TimestampWithTimeZoneArray = "_timestamptz";
        public const string IntervalArray = "_interval";
        public const string UuidArray = "_uuid";
        public const string ByteaArray = "_bytea";
        public const string JsonArray = "_json";
        public const string JsonBinaryArray = "_jsonb";
        public const string XmlArray = "_xml";
        public const string MoneyArray = "_money";
        public const string InetArray = "_inet";
        public const string CidrArray = "_cidr";
        public const string MacAddressArray = "_macaddr";
    }

    // islevi: SQL Server sys.types.name degerlerinin desteklenen alt kumesini sabitler.
    // sistemdeki gorevi: SqlServerEngineTypeMapProvider tablosunun provider-katalog sozlesmesini tasir.
    public static class SqlServer
    {
        public const string Char = DatabaseMetadataCatalogConstants.SqlServer.CharTypeName;
        public const string VarChar = DatabaseMetadataCatalogConstants.SqlServer.VarCharTypeName;
        public const string NChar = DatabaseMetadataCatalogConstants.SqlServer.NCharTypeName;
        public const string NVarChar = DatabaseMetadataCatalogConstants.SqlServer.NVarCharTypeName;
        public const string SysName = "sysname";
        public const string Text = "text";
        public const string NText = "ntext";
        public const string TinyInt = "tinyint";
        public const string SmallInt = "smallint";
        public const string Int = "int";
        public const string BigInt = "bigint";
        public const string Decimal = DatabaseMetadataCatalogConstants.SqlServer.DecimalTypeName;
        public const string Numeric = DatabaseMetadataCatalogConstants.SqlServer.NumericTypeName;
        public const string Real = "real";
        public const string Float = "float";
        public const string Bit = "bit";
        public const string Date = "date";
        public const string Time = "time";
        public const string SmallDateTime = "smalldatetime";
        public const string DateTime = "datetime";
        public const string DateTime2 = "datetime2";
        public const string DateTimeOffset = "datetimeoffset";
        public const string UniqueIdentifier = "uniqueidentifier";
        public const string Binary = DatabaseMetadataCatalogConstants.SqlServer.BinaryTypeName;
        public const string VarBinary = DatabaseMetadataCatalogConstants.SqlServer.VarBinaryTypeName;
        public const string Image = "image";
        public const string Timestamp = "timestamp";
        public const string RowVersion = "rowversion";
        public const string Json = "json";
        public const string Xml = "xml";
        public const string Money = "money";
        public const string SmallMoney = "smallmoney";
        public const string Geometry = "geometry";
        public const string Geography = "geography";
        public const string SqlVariant = "sql_variant";
    }
}
