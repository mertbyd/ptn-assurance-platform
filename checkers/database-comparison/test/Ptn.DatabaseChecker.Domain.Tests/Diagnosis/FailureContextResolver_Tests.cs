using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.Diagnosis;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Ptn.DatabaseChecker.Models.Assertions;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Diagnosis;

// islevi: FailureContextResolver'in provider adlarini canli katalogda dogrulamadan tutmadigini test eder.
// sistemdeki gorevi: Katalogda olmayan constraint adinin atilmasi ve kimlik guveninin Low'a dusmesi icin regresyon kanitidir.
public class FailureContextResolver_Tests
{
    // islevi: Katalogda bulunmayan provider constraint adinin rapordan atilip guveni dusurdugunu dogrular.
    [Fact]
    public async Task Unknown_Constraint_Name_Should_Be_Discarded_And_Confidence_Downgraded()
    {
        var schemaManager = Substitute.For<SchemaDiscoveryManager>();
        schemaManager.ReadSnapshotAsync(
                Arg.Any<DatabaseConnection>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(CreateSnapshot());
        var dataManager = Substitute.For<DatabaseDataComparisonManager>();
        dataManager.ResolveAssertionStructureAsync(
                Arg.Any<DatabaseConnection>(), "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        var resolver = new FailureContextResolver(schemaManager, dataManager, new FindingValueRedactor());
        var identity = CreateIdentity();
        var signal = CreateSignal();

        var context = await resolver.ResolveAsync(
            CreateConnection(), signal, identity,
            new ValueRetentionPolicy(ValueRetentionModeCodes.None, string.Empty));

        context.Location.ConstraintName.ShouldBeNull();
        identity.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Low);
    }

    // islevi: None retention politikasinda failed-expectation kaynak degerlerinin context'ten okunamadigini dogrular.
    [Fact]
    public async Task None_Retention_Should_Remove_Source_Values()
    {
        var schemaManager = Substitute.For<SchemaDiscoveryManager>();
        schemaManager.ReadSnapshotAsync(
                Arg.Any<DatabaseConnection>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(CreateSnapshot());
        var dataManager = Substitute.For<DatabaseDataComparisonManager>();
        dataManager.ResolveAssertionStructureAsync(
                Arg.Any<DatabaseConnection>(), "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        var resolver = new FailureContextResolver(schemaManager, dataManager, new FindingValueRedactor());
        var signal = CreateAssertionSignal();
        var identity = FailureIdentity.FromAssertion(DatabaseEngineCodes.PostgreSql, signal.Assertion!);

        var context = await resolver.ResolveAsync(
            CreateConnection(), signal, identity,
            new ValueRetentionPolicy(ValueRetentionModeCodes.None, string.Empty));

        context.FailedExpectations.Single().ExpectedValue.ShouldBeNull();
        context.FailedExpectations.Single().ObservedValue.ShouldBeNull();
    }

    // islevi: Test icin orders tablosu bulunan fakat provider constraint adini icermeyen snapshot kurar.
    private static SchemaSnapshotModel CreateSnapshot()
        => new()
        {
            Tables = new()
            {
                new SchemaTableModel
                {
                    Schema = "public",
                    Name = "orders",
                    Columns = new() { new SchemaColumnModel { Name = "id" } }
                }
            }
        };

    // islevi: Resolver'in probe structure adimini gecmesi icin tek PK yapisi kurar.
    private static TableDataStructureModel CreateStructure()
        => new()
        {
            SchemaName = "public",
            TableName = "orders",
            ColumnNames = new() { "id" },
            PrimaryKeyColumns = new() { "id" }
        };

    // islevi: Katalogda bulunmayacak constraint adli yuksek guvenli provider kimligi kurar.
    private static FailureIdentity CreateIdentity()
        => new()
        {
            EngineCode = DatabaseEngineCodes.PostgreSql,
            ConfidenceCode = DiagnosisConfidenceCodes.High,
            ObjectReferences = new()
            {
                new ObjectReference
                {
                    SchemaName = "public",
                    TableName = "orders",
                    ConstraintName = "fk_missing"
                }
            }
        };

    // islevi: Resolver testine yapilandirilmis PostgreSQL database-exception sinyali verir.
    private static FailureSignal CreateSignal()
        => new()
        {
            DbException = new FailureSignal.DatabaseExceptionFailureSignal
            {
                EngineCode = DatabaseEngineCodes.PostgreSql,
                SqlState = PostgreSqlSqlStateCodes.ForeignKeyViolation
            }
        };

    // islevi: Redaction testine kaynak expected/observed degerleri dolu assertion sinyali kurar.
    private static FailureSignal CreateAssertionSignal()
        => new()
        {
            Assertion = new FailureSignal.AssertionFailureSignal
            {
                SchemaName = "public",
                TableName = "orders",
                OutcomeCode = AssertionOutcomeCodes.ValueMismatch,
                KeyValues = { ["id"] = "42" },
                FailedExpectations = new()
                {
                    new FailedExpectation
                    {
                        ColumnName = "status",
                        ExpectedValue = "secret-expected",
                        ObservedValue = "secret-observed"
                    }
                }
            }
        };

    // islevi: PostgreSQL engine navigation'i yuklu kayitli test baglantisi kurar.
    private static DatabaseConnection CreateConnection()
        => new(Guid.NewGuid())
        {
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL")
        };
}
