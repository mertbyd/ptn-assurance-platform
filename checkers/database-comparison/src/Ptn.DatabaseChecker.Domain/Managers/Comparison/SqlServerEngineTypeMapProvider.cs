using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: SQL Server ham katalog tip adlarini kapali kanonik aile ve fidelity ciftine esler.
// sistemdeki gorevi: SQL Server discovery akisini karsilastirma oncesinde tek sefer normalize eden IEngineTypeMapProvider bilesenidir.
/// <summary>SQL Server katalog tiplerini kanonik karsilastirma tiplerine esler.</summary>
public class SqlServerEngineTypeMapProvider : EngineTypeMapProviderBase, ITransientDependency
{
    private static readonly CanonicalTypeMapping StringMapping = Exact(CanonicalDataTypeCodes.String);
    private static readonly CanonicalTypeMapping TextMapping = Exact(CanonicalDataTypeCodes.Text);
    private static readonly CanonicalTypeMapping SmallIntegerMapping = Exact(CanonicalDataTypeCodes.SmallInteger);
    private static readonly CanonicalTypeMapping ApproximateSmallIntegerMapping = Approximate(CanonicalDataTypeCodes.SmallInteger);
    private static readonly CanonicalTypeMapping IntegerMapping = Exact(CanonicalDataTypeCodes.Integer);
    private static readonly CanonicalTypeMapping BigIntegerMapping = Exact(CanonicalDataTypeCodes.BigInteger);
    private static readonly CanonicalTypeMapping DecimalMapping = Exact(CanonicalDataTypeCodes.Decimal);
    private static readonly CanonicalTypeMapping FloatMapping = Exact(CanonicalDataTypeCodes.Float);
    private static readonly CanonicalTypeMapping ApproximateDoubleMapping = Approximate(CanonicalDataTypeCodes.Double);
    private static readonly CanonicalTypeMapping BooleanMapping = Exact(CanonicalDataTypeCodes.Boolean);
    private static readonly CanonicalTypeMapping DateMapping = Exact(CanonicalDataTypeCodes.Date);
    private static readonly CanonicalTypeMapping TimeMapping = Exact(CanonicalDataTypeCodes.Time);
    private static readonly CanonicalTypeMapping TimestampMapping = Exact(CanonicalDataTypeCodes.Timestamp);
    private static readonly CanonicalTypeMapping ApproximateTimestampMapping = Approximate(CanonicalDataTypeCodes.Timestamp);
    private static readonly CanonicalTypeMapping TimestampWithTimeZoneMapping = Exact(CanonicalDataTypeCodes.TimestampWithTimeZone);
    private static readonly CanonicalTypeMapping UuidMapping = Exact(CanonicalDataTypeCodes.Uuid);
    private static readonly CanonicalTypeMapping BinaryMapping = Exact(CanonicalDataTypeCodes.Binary);
    private static readonly CanonicalTypeMapping ApproximateBinaryMapping = Approximate(CanonicalDataTypeCodes.Binary);
    private static readonly CanonicalTypeMapping JsonMapping = Exact(CanonicalDataTypeCodes.Json);
    private static readonly CanonicalTypeMapping XmlMapping = Exact(CanonicalDataTypeCodes.Xml);
    private static readonly CanonicalTypeMapping ApproximateMoneyMapping = Approximate(CanonicalDataTypeCodes.Money);
    private static readonly CanonicalTypeMapping GeometryMapping = Exact(CanonicalDataTypeCodes.Geometry);

    // Ham tip tablosu yalniz semantic karsiligi bilinen SQL Server katalog isimlerini icerir; sql_variant bilerek disarida kalir.
    private static readonly IReadOnlyDictionary<string, CanonicalTypeMapping> Mappings =
        new Dictionary<string, CanonicalTypeMapping>(StringComparer.OrdinalIgnoreCase)
        {
            [EngineDataTypeNameCodes.SqlServer.Char] = StringMapping,
            [EngineDataTypeNameCodes.SqlServer.VarChar] = StringMapping,
            [EngineDataTypeNameCodes.SqlServer.NChar] = StringMapping,
            [EngineDataTypeNameCodes.SqlServer.NVarChar] = StringMapping,
            [EngineDataTypeNameCodes.SqlServer.SysName] = StringMapping,
            [EngineDataTypeNameCodes.SqlServer.Text] = TextMapping,
            [EngineDataTypeNameCodes.SqlServer.NText] = TextMapping,
            [EngineDataTypeNameCodes.SqlServer.TinyInt] = ApproximateSmallIntegerMapping,
            [EngineDataTypeNameCodes.SqlServer.SmallInt] = SmallIntegerMapping,
            [EngineDataTypeNameCodes.SqlServer.Int] = IntegerMapping,
            [EngineDataTypeNameCodes.SqlServer.BigInt] = BigIntegerMapping,
            [EngineDataTypeNameCodes.SqlServer.Decimal] = DecimalMapping,
            [EngineDataTypeNameCodes.SqlServer.Numeric] = DecimalMapping,
            [EngineDataTypeNameCodes.SqlServer.Real] = FloatMapping,
            [EngineDataTypeNameCodes.SqlServer.Float] = ApproximateDoubleMapping,
            [EngineDataTypeNameCodes.SqlServer.Bit] = BooleanMapping,
            [EngineDataTypeNameCodes.SqlServer.Date] = DateMapping,
            [EngineDataTypeNameCodes.SqlServer.Time] = TimeMapping,
            [EngineDataTypeNameCodes.SqlServer.SmallDateTime] = ApproximateTimestampMapping,
            [EngineDataTypeNameCodes.SqlServer.DateTime] = ApproximateTimestampMapping,
            [EngineDataTypeNameCodes.SqlServer.DateTime2] = TimestampMapping,
            [EngineDataTypeNameCodes.SqlServer.DateTimeOffset] = TimestampWithTimeZoneMapping,
            [EngineDataTypeNameCodes.SqlServer.UniqueIdentifier] = UuidMapping,
            [EngineDataTypeNameCodes.SqlServer.Binary] = BinaryMapping,
            [EngineDataTypeNameCodes.SqlServer.VarBinary] = BinaryMapping,
            [EngineDataTypeNameCodes.SqlServer.Image] = ApproximateBinaryMapping,
            [EngineDataTypeNameCodes.SqlServer.Timestamp] = ApproximateBinaryMapping,
            [EngineDataTypeNameCodes.SqlServer.RowVersion] = ApproximateBinaryMapping,
            [EngineDataTypeNameCodes.SqlServer.Json] = JsonMapping,
            [EngineDataTypeNameCodes.SqlServer.Xml] = XmlMapping,
            [EngineDataTypeNameCodes.SqlServer.Money] = ApproximateMoneyMapping,
            [EngineDataTypeNameCodes.SqlServer.SmallMoney] = ApproximateMoneyMapping,
            [EngineDataTypeNameCodes.SqlServer.Geometry] = GeometryMapping,
            [EngineDataTypeNameCodes.SqlServer.Geography] = GeometryMapping
        };

    // islevi: 0.1.x EF paketi tip uyumluluk kabugunun ayni tek esleme tablosunu kullanmasini saglar.
    internal static IReadOnlyDictionary<string, CanonicalTypeMapping> CompatibilityMappings => Mappings;

    protected override IReadOnlyDictionary<string, CanonicalTypeMapping> TypeMappings => Mappings;

    /// <summary>SQL Server kararli motor kodunu dondurur.</summary>
    public override string EngineCode => DatabaseEngineCodes.SqlServer;
}
