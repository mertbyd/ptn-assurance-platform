using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: PostgreSQL ham katalog tip adlarini kapali kanonik aile ve fidelity ciftine esler.
// sistemdeki gorevi: PostgreSQL discovery akisini karsilastirma oncesinde tek sefer normalize eden IEngineTypeMapProvider bilesenidir.
/// <summary>PostgreSQL katalog tiplerini kanonik karsilastirma tiplerine esler.</summary>
public class PostgreSqlEngineTypeMapProvider : EngineTypeMapProviderBase, ITransientDependency
{
    private static readonly CanonicalTypeMapping StringMapping = Exact(CanonicalDataTypeCodes.String);
    private static readonly CanonicalTypeMapping ApproximateStringMapping = Approximate(CanonicalDataTypeCodes.String);
    private static readonly CanonicalTypeMapping TextMapping = Exact(CanonicalDataTypeCodes.Text);
    private static readonly CanonicalTypeMapping SmallIntegerMapping = Exact(CanonicalDataTypeCodes.SmallInteger);
    private static readonly CanonicalTypeMapping IntegerMapping = Exact(CanonicalDataTypeCodes.Integer);
    private static readonly CanonicalTypeMapping BigIntegerMapping = Exact(CanonicalDataTypeCodes.BigInteger);
    private static readonly CanonicalTypeMapping DecimalMapping = Exact(CanonicalDataTypeCodes.Decimal);
    private static readonly CanonicalTypeMapping FloatMapping = Exact(CanonicalDataTypeCodes.Float);
    private static readonly CanonicalTypeMapping DoubleMapping = Exact(CanonicalDataTypeCodes.Double);
    private static readonly CanonicalTypeMapping BooleanMapping = Exact(CanonicalDataTypeCodes.Boolean);
    private static readonly CanonicalTypeMapping DateMapping = Exact(CanonicalDataTypeCodes.Date);
    private static readonly CanonicalTypeMapping TimeMapping = Exact(CanonicalDataTypeCodes.Time);
    private static readonly CanonicalTypeMapping ApproximateTimeMapping = Approximate(CanonicalDataTypeCodes.Time);
    private static readonly CanonicalTypeMapping TimestampMapping = Exact(CanonicalDataTypeCodes.Timestamp);
    private static readonly CanonicalTypeMapping TimestampWithTimeZoneMapping = Exact(CanonicalDataTypeCodes.TimestampWithTimeZone);
    private static readonly CanonicalTypeMapping IntervalMapping = Exact(CanonicalDataTypeCodes.Interval);
    private static readonly CanonicalTypeMapping UuidMapping = Exact(CanonicalDataTypeCodes.Uuid);
    private static readonly CanonicalTypeMapping BinaryMapping = Exact(CanonicalDataTypeCodes.Binary);
    private static readonly CanonicalTypeMapping ApproximateBinaryMapping = Approximate(CanonicalDataTypeCodes.Binary);
    private static readonly CanonicalTypeMapping JsonMapping = Exact(CanonicalDataTypeCodes.Json);
    private static readonly CanonicalTypeMapping ApproximateJsonMapping = Approximate(CanonicalDataTypeCodes.Json);
    private static readonly CanonicalTypeMapping XmlMapping = Exact(CanonicalDataTypeCodes.Xml);
    private static readonly CanonicalTypeMapping MoneyMapping = Exact(CanonicalDataTypeCodes.Money);
    private static readonly CanonicalTypeMapping NetworkMapping = Exact(CanonicalDataTypeCodes.Network);
    private static readonly CanonicalTypeMapping GeometryMapping = Exact(CanonicalDataTypeCodes.Geometry);
    private static readonly CanonicalTypeMapping UnknownMapping = Exact(CanonicalDataTypeCodes.Unknown);
    private static readonly CanonicalTypeMapping ArrayMapping = Exact(CanonicalDataTypeCodes.Array);

    // Ham tip tablosu provider katalog isimleriyle okunur; bilinmeyen extension/domain tipleri bilerek tablo disinda kalir.
    private static readonly IReadOnlyDictionary<string, CanonicalTypeMapping> Mappings =
        new Dictionary<string, CanonicalTypeMapping>(StringComparer.OrdinalIgnoreCase)
        {
            [EngineDataTypeNameCodes.PostgreSql.VarChar] = StringMapping,
            [EngineDataTypeNameCodes.PostgreSql.CharacterVarying] = StringMapping,
            [EngineDataTypeNameCodes.PostgreSql.BpChar] = StringMapping,
            [EngineDataTypeNameCodes.PostgreSql.Character] = StringMapping,
            [EngineDataTypeNameCodes.PostgreSql.Char] = StringMapping,
            [EngineDataTypeNameCodes.PostgreSql.Name] = StringMapping,
            [EngineDataTypeNameCodes.PostgreSql.Citext] = ApproximateStringMapping,
            [EngineDataTypeNameCodes.PostgreSql.Text] = TextMapping,
            [EngineDataTypeNameCodes.PostgreSql.SmallInt] = SmallIntegerMapping,
            [EngineDataTypeNameCodes.PostgreSql.SmallInteger] = SmallIntegerMapping,
            [EngineDataTypeNameCodes.PostgreSql.Integer] = IntegerMapping,
            [EngineDataTypeNameCodes.PostgreSql.IntegerAlias] = IntegerMapping,
            [EngineDataTypeNameCodes.PostgreSql.BigInt] = BigIntegerMapping,
            [EngineDataTypeNameCodes.PostgreSql.BigInteger] = BigIntegerMapping,
            [EngineDataTypeNameCodes.PostgreSql.Numeric] = DecimalMapping,
            [EngineDataTypeNameCodes.PostgreSql.Decimal] = DecimalMapping,
            [EngineDataTypeNameCodes.PostgreSql.Real] = FloatMapping,
            [EngineDataTypeNameCodes.PostgreSql.RealAlias] = FloatMapping,
            [EngineDataTypeNameCodes.PostgreSql.DoublePrecision] = DoubleMapping,
            [EngineDataTypeNameCodes.PostgreSql.DoublePrecisionAlias] = DoubleMapping,
            [EngineDataTypeNameCodes.PostgreSql.Boolean] = BooleanMapping,
            [EngineDataTypeNameCodes.PostgreSql.BooleanAlias] = BooleanMapping,
            [EngineDataTypeNameCodes.PostgreSql.Date] = DateMapping,
            [EngineDataTypeNameCodes.PostgreSql.Time] = TimeMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimeWithoutTimeZone] = TimeMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimeWithTimeZone] = ApproximateTimeMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimeWithTimeZoneAlias] = ApproximateTimeMapping,
            [EngineDataTypeNameCodes.PostgreSql.Timestamp] = TimestampMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimestampWithoutTimeZone] = TimestampMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimestampWithTimeZone] = TimestampWithTimeZoneMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimestampWithTimeZoneAlias] = TimestampWithTimeZoneMapping,
            [EngineDataTypeNameCodes.PostgreSql.Interval] = IntervalMapping,
            [EngineDataTypeNameCodes.PostgreSql.Uuid] = UuidMapping,
            [EngineDataTypeNameCodes.PostgreSql.Bytea] = BinaryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Bit] = ApproximateBinaryMapping,
            [EngineDataTypeNameCodes.PostgreSql.VarBit] = ApproximateBinaryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Json] = JsonMapping,
            [EngineDataTypeNameCodes.PostgreSql.JsonBinary] = JsonMapping,
            [EngineDataTypeNameCodes.PostgreSql.JsonPath] = ApproximateJsonMapping,
            [EngineDataTypeNameCodes.PostgreSql.Xml] = XmlMapping,
            [EngineDataTypeNameCodes.PostgreSql.Money] = MoneyMapping,
            [EngineDataTypeNameCodes.PostgreSql.Inet] = NetworkMapping,
            [EngineDataTypeNameCodes.PostgreSql.Cidr] = NetworkMapping,
            [EngineDataTypeNameCodes.PostgreSql.MacAddress] = NetworkMapping,
            [EngineDataTypeNameCodes.PostgreSql.MacAddress8] = NetworkMapping,
            [EngineDataTypeNameCodes.PostgreSql.Point] = GeometryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Line] = GeometryMapping,
            [EngineDataTypeNameCodes.PostgreSql.LineSegment] = GeometryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Box] = GeometryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Path] = GeometryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Polygon] = GeometryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Circle] = GeometryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Geometry] = GeometryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Geography] = GeometryMapping,
            [EngineDataTypeNameCodes.PostgreSql.Unknown] = UnknownMapping,
            [EngineDataTypeNameCodes.PostgreSql.BooleanArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.SmallIntegerArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.IntegerArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.BigIntegerArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.NumericArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.RealArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.DoubleArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.TextArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.VarCharArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.BpCharArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.DateArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimeArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimeWithTimeZoneArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimestampArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.TimestampWithTimeZoneArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.IntervalArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.UuidArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.ByteaArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.JsonArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.JsonBinaryArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.XmlArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.MoneyArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.InetArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.CidrArray] = ArrayMapping,
            [EngineDataTypeNameCodes.PostgreSql.MacAddressArray] = ArrayMapping
        };

    // islevi: 0.1.x EF paketi tip uyumluluk kabugunun ayni tek esleme tablosunu kullanmasini saglar.
    internal static IReadOnlyDictionary<string, CanonicalTypeMapping> CompatibilityMappings => Mappings;

    protected override IReadOnlyDictionary<string, CanonicalTypeMapping> TypeMappings => Mappings;

    /// <summary>PostgreSQL kararli motor kodunu dondurur.</summary>
    public override string EngineCode => DatabaseEngineCodes.PostgreSql;
}
