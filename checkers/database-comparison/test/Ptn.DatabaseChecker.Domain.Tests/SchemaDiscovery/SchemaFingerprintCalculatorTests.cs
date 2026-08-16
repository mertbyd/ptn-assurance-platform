using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.SchemaDiscovery;

// islevi: Sema muhrunun kararliligini, kanonikligini ve dal cozunurlugunu dogrular.
// sistemdeki gorevi: Zaman, okuma sirasi ve ordinal pozisyonun muhre sizmasini engeller; sizsaydi kosum anindaki kayma tespiti her koc'ta yanlis alarm uretirdi.
public class SchemaFingerprintCalculatorTests
{
    private const string CustomersAddress = "public.customers";
    private const string OrdersAddress = "public.orders";

    [Fact]
    public void Same_Structure_Should_Produce_The_Same_Seal_Twice()
    {
        var calculator = CreateCalculator();

        var first = calculator.Calculate(CreateSnapshot());
        var second = calculator.Calculate(CreateSnapshot());

        second.SnapshotFingerprint.ShouldBe(first.SnapshotFingerprint);
        second.Schemas.Select(entry => entry.Fingerprint)
            .ShouldBe(first.Schemas.Select(entry => entry.Fingerprint));
        second.Tables.Select(entry => entry.Fingerprint)
            .ShouldBe(first.Tables.Select(entry => entry.Fingerprint));
    }

    [Fact]
    public void Collection_Time_Should_Not_Enter_The_Seal()
    {
        var calculator = CreateCalculator();
        var later = CreateSnapshot();
        later.CollectedAt = later.CollectedAt.AddHours(7);

        var baseline = calculator.Calculate(CreateSnapshot());
        var shifted = calculator.Calculate(later);

        shifted.SnapshotFingerprint.ShouldBe(baseline.SnapshotFingerprint);
    }

    [Fact]
    public void Catalog_Read_Order_And_Ordinal_Position_Should_Not_Enter_The_Seal()
    {
        var calculator = CreateCalculator();
        var reordered = CreateSnapshot();
        reordered.Tables.Reverse();
        ReverseTableChildren(reordered.Tables[0]);
        ReverseTableChildren(reordered.Tables[1]);
        reordered.Objects.Reverse();

        var baseline = calculator.Calculate(CreateSnapshot());
        var shuffled = calculator.Calculate(reordered);

        shuffled.SnapshotFingerprint.ShouldBe(baseline.SnapshotFingerprint);
        Branch(shuffled.Tables, CustomersAddress).ShouldBe(Branch(baseline.Tables, CustomersAddress));
        shuffled.Tables.Select(entry => entry.Name).ShouldBe([CustomersAddress, OrdersAddress]);
    }

    [Fact]
    public void Changed_Column_Type_Should_Move_Only_Its_Own_Table_Branch()
    {
        var calculator = CreateCalculator();
        var widened = CreateSnapshot();
        FindColumn(widened, "customers", "name").RawDataType = "varchar(100)";

        var baseline = calculator.Calculate(CreateSnapshot());
        var current = calculator.Calculate(widened);

        current.SnapshotFingerprint.ShouldNotBe(baseline.SnapshotFingerprint);
        Branch(current.Tables, CustomersAddress).ShouldNotBe(Branch(baseline.Tables, CustomersAddress));
        Branch(current.Tables, OrdersAddress).ShouldBe(Branch(baseline.Tables, OrdersAddress));
    }

    [Fact]
    public void Added_Column_Should_Leave_An_Unrelated_Table_Branch_Untouched()
    {
        var calculator = CreateCalculator();
        var extended = CreateSnapshot();
        FindTable(extended, "customers").Columns.Add(
            Column("created_at", 3, "timestamptz", CanonicalDataTypeCodes.TimestampWithTimeZone));

        var baseline = calculator.Calculate(CreateSnapshot());
        var current = calculator.Calculate(extended);

        Branch(current.Tables, OrdersAddress).ShouldBe(Branch(baseline.Tables, OrdersAddress));
        Branch(current.Tables, CustomersAddress).ShouldNotBe(Branch(baseline.Tables, CustomersAddress));
        current.SnapshotFingerprint.ShouldNotBe(baseline.SnapshotFingerprint);
    }

    [Fact]
    public void Every_Seal_Should_Be_64_Uppercase_Hexadecimal_Characters()
    {
        var result = CreateCalculator().Calculate(CreateSnapshot());

        var seals = result.Schemas.Concat(result.Tables)
            .Select(entry => entry.Fingerprint)
            .Append(result.SnapshotFingerprint)
            .ToList();

        seals.ShouldAllBe(seal => seal.Length == SchemaFingerprintConsts.FingerprintLength);
        seals.ShouldAllBe(seal => IsUppercaseHexadecimal(seal));
        result.AlgorithmCode.ShouldBe(SchemaFingerprintConsts.AlgorithmCode);
        result.AlgorithmVersion.ShouldBe(SchemaFingerprintConsts.AlgorithmVersion);
    }

    // islevi: Muhrun kararli buyuk harfli onaltilik bicimde uretildigini bildirir.
    private static bool IsUppercaseHexadecimal(string seal)
        => seal.All(character => character is >= '0' and <= '9' || character is >= 'A' and <= 'F');

    // islevi: Muhur testleri icin hesaplayiciyi mevcut sema normalizasyon sahibiyle kurar.
    private static SchemaFingerprintCalculator CreateCalculator()
        => new(new SchemaDefinitionNormalizer());

    // islevi: Iki tablo, bir view ve tam kolon derinligi tasiyan referans fotografi kurar.
    private static SchemaSnapshotModel CreateSnapshot()
        => new()
        {
            EngineCode = DatabaseEngineCodes.PostgreSql,
            DatabaseName = "assurance",
            DatabaseCollationName = "en_US.utf8",
            CollationProviderCode = "c",
            CollectedAt = new DateTime(2026, 8, 14, 9, 0, 0, DateTimeKind.Utc),
            Tables = [CreateCustomers(), CreateOrders()],
            Objects =
            [
                CreateObject("active_customers", "SELECT id FROM customers WHERE is_active"),
                CreateObject("recent_orders", "SELECT id FROM orders")
            ]
        };

    // islevi: Kolon, kisit, index ve trigger derinligi olan referans tabloyu kurar.
    private static SchemaTableModel CreateCustomers()
        => new()
        {
            Schema = "public",
            Name = "customers",
            Columns =
            [
                Column("id", 1, "integer", CanonicalDataTypeCodes.Integer, isIdentity: true),
                Column("name", 2, "varchar(50)", CanonicalDataTypeCodes.String, maxLength: 50)
            ],
            Indexes =
            [
                new SchemaIndexModel
                {
                    Name = "pk_customers",
                    IsUnique = true,
                    IsPrimaryKey = true,
                    Columns = ["id"]
                }
            ],
            Constraints =
            [
                new SchemaConstraintModel
                {
                    Name = "pk_customers",
                    TypeCode = SchemaConstraintTypeCodes.PrimaryKey,
                    Columns = ["id"]
                }
            ],
            Triggers =
            [
                new SchemaTriggerModel { Name = "customers_audit", Definition = "CREATE TRIGGER customers_audit" }
            ]
        };

    // islevi: Karsilastirma tarafi degismeyen ikinci referans tabloyu kurar.
    private static SchemaTableModel CreateOrders()
        => new()
        {
            Schema = "public",
            Name = "orders",
            Columns = [Column("id", 1, "integer", CanonicalDataTypeCodes.Integer, isIdentity: true)],
            Constraints =
            [
                new SchemaConstraintModel
                {
                    Name = "fk_orders_customers",
                    TypeCode = SchemaConstraintTypeCodes.ForeignKey,
                    Columns = ["customer_id"],
                    ReferencedTable = "public.customers",
                    ReferencedColumns = ["id"]
                }
            ]
        };

    // islevi: Tek tablo-disi sema nesnesini kurar.
    private static SchemaObjectDefinitionModel CreateObject(string name, string definition)
        => new()
        {
            Schema = "public",
            Name = name,
            ObjectTypeCode = SchemaObjectTypeCodes.View,
            Definition = definition
        };

    // islevi: Referans fotograf icin tek kolon kurar.
    private static SchemaColumnModel Column(
        string name,
        int ordinal,
        string rawDataType,
        string canonicalDataType,
        int? maxLength = null,
        bool isIdentity = false)
        => new()
        {
            Name = name,
            Ordinal = ordinal,
            RawDataType = rawDataType,
            CanonicalDataType = canonicalDataType,
            MaxLength = maxLength,
            IsIdentity = isIdentity
        };

    // islevi: Katalog okuma sirasi degisimini tablonun tum alt listelerinde taklit eder ve ordinal pozisyonu kaydirir.
    private static void ReverseTableChildren(SchemaTableModel table)
    {
        table.Columns.Reverse();
        table.Constraints.Reverse();
        table.Indexes.Reverse();
        table.Triggers.Reverse();
        table.Columns.ForEach(column => column.Ordinal = table.Columns.Count - column.Ordinal + 1);
    }

    // islevi: Dal listesinden adrese karsilik gelen muhru okur.
    private static string Branch(List<SchemaFingerprintEntryModel> entries, string address)
        => entries.Single(entry => entry.Name == address).Fingerprint;

    // islevi: Fotograftaki hedef tabloyu adiyla bulur.
    private static SchemaTableModel FindTable(SchemaSnapshotModel snapshot, string tableName)
        => snapshot.Tables.Single(table => table.Name == tableName);

    // islevi: Fotograftaki hedef kolonu tablo ve kolon adiyla bulur.
    private static SchemaColumnModel FindColumn(
        SchemaSnapshotModel snapshot,
        string tableName,
        string columnName)
        => FindTable(snapshot, tableName).Columns.Single(column => column.Name == columnName);
}
