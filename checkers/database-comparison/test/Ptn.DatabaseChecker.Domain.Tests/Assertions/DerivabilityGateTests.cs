using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Managers.Assertions;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Assertions;

// islevi: DB derivability kapisinin tablo, kolon, unique anahtar, matcher-tip ve tam basari outcome'larini dogrular.
// sistemdeki gorevi: Her assertion item'inin ilk basarisiz soruda tek fail-closed sonuc tasimasini korur.
public class DerivabilityGateTests
{
    [Fact]
    public async Task Gate_Should_Return_One_Outcome_For_Each_Derivability_Question()
    {
        var discovery = Substitute.For<SchemaDiscoveryManager>();
        discovery.DescribeTablesAsync(
                Arg.Any<DatabaseConnection>(),
                Arg.Any<List<ComparisonTableIdentifierModel>>(),
                Arg.Any<CancellationToken>())
            .Returns([Description("users"), Description("missing_column"), Description("non_unique"), Description("mismatch")]);
        var manager = new AssertionDerivabilityManager(
            discovery,
            new ColumnTypeConfidenceResolver());
        var connection = new DatabaseConnection(Guid.NewGuid());

        var result = await manager.ValidateAsync(connection, CreateRequest());

        result.Assertions.Select(item => item.OutcomeCode).ShouldBe(
            [
                AssertionDerivabilityCodes.TableNotFound,
                AssertionDerivabilityCodes.ColumnNotFound,
                AssertionDerivabilityCodes.KeyNotUnique,
                AssertionDerivabilityCodes.MatcherTypeMismatch,
                AssertionDerivabilityCodes.Derivable
            ]);
    }

    // islevi: Bes kapali outcome'u girdi sirasiyla uretecek assertion adreslerini kurar.
    private static DerivabilityRequest CreateRequest()
        => new()
        {
            Assertions =
            [
                Address("ghost", ["id"], ["amount"], MatcherKindCodes.Equals),
                Address("missing_column", ["id"], ["unknown"], MatcherKindCodes.Equals),
                Address("non_unique", ["status"], ["amount"], MatcherKindCodes.Equals),
                Address("mismatch", ["id"], ["status"], MatcherKindCodes.WithinTolerance),
                Address("users", ["id"], ["amount"], MatcherKindCodes.WithinTolerance)
            ]
        };

    // islevi: Tek tablo hedefi icin derivability assertion adresi kurar.
    private static DerivabilityAddress Address(
        string table,
        List<string> keyColumns,
        List<string> expectedColumns,
        string matcherCode)
        => new()
        {
            SchemaName = "public",
            TableName = table,
            KeyColumns = keyColumns,
            ExpectedColumns = expectedColumns,
            MatcherCode = matcherCode,
            CardinalityKindCode = CardinalityKindCodes.Exactly
        };

    // islevi: Id PK, string status ve decimal amount kolonlu tablo tanimi kurar.
    private static TableDescriptionModel Description(string table)
        => new()
        {
            SchemaName = "public",
            TableName = table,
            Columns =
            [
                new TableDescriptionColumnModel { Name = "id", CanonicalDataTypeCode = CanonicalDataTypeCodes.Integer },
                new TableDescriptionColumnModel { Name = "status", CanonicalDataTypeCode = CanonicalDataTypeCodes.String },
                new TableDescriptionColumnModel { Name = "amount", CanonicalDataTypeCode = CanonicalDataTypeCodes.Decimal }
            ],
            PrimaryKey = new TableKeyDefinitionModel { Name = "pk", Columns = ["id"] }
        };
}
