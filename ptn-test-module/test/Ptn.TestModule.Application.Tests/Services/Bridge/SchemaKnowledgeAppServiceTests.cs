using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Services.Bridge;
using Ptn.DatabaseChecker.Dtos.SchemaDiscovery;
using Ptn.DatabaseChecker.Services.SchemaDiscovery;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.FluentValidation.Bridge.Database;
using Ptn.TestModule.Managers.Bridge;
using Shouldly;
using Xunit;
using ForeignKeyDirectionCodes = Ptn.DatabaseChecker.Constants.Comparison.ForeignKeyDirectionCodes;
using SchemaLintWarningCodes = Ptn.DatabaseChecker.Constants.Comparison.SchemaLintWarningCodes;
using BridgeTableQueryDto = Ptn.TestModule.Dtos.Bridge.Database.TableQueryDto;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Schema adapter'inin DB konum semantigi, anahtar cevirisi ve kanonik fingerprint'ini dogrular.
// sistemdeki gorevi: Provider liste sirasi degistiginde profil muhrunun yanlis drift uretmesini engeller.
public class SchemaKnowledgeAppServiceTests
{
    // Ayni tablo ve kolon kumeleri farkli sirada geldiginde ayni SHA-256 fingerprint'i uretir.
    [Fact]
    public async Task Should_compute_order_independent_schema_fingerprint()
    {
        var service = new SchemaDiscoveryFake(
            CreateSnapshot(reverse: false),
            CreateSnapshot(reverse: true));
        var schemaService = new SchemaKnowledgeAppService(
            service,
            new SchemaKnowledgeManager(),
            new TableQueryDtoValidator());

        var first = await schemaService.GetSchemaFingerprintAsync(Guid.NewGuid(), CancellationToken.None);
        var second = await schemaService.GetSchemaFingerprintAsync(Guid.NewGuid(), CancellationToken.None);

        first.ShouldBe(second);
        first.ShouldStartWith("sha256:");
    }

    // DescribeTable sonucunu DbSchemaName ile tasir ve PK turunu kapali kopru koduna cevirir.
    [Fact]
    public async Task Should_map_database_schema_name_and_primary_key()
    {
        var service = new SchemaDiscoveryFake(CreateSnapshot(reverse: false))
        {
            TableDescription = new TableDescriptionDto
            {
                SchemaName = "identity",
                TableName = "users",
                PrimaryKey = new TableKeyDefinitionDto { Columns = ["id"] },
                ForeignKeyNeighbors =
                [
                    new ForeignKeyNeighborDto
                    {
                        DirectionCode = ForeignKeyDirectionCodes.Incoming,
                        ConstraintName = "fk_tickets_user",
                        SchemaName = "support",
                        TableName = "tickets",
                        LocalColumns = ["id"],
                        NeighborColumns = ["user_id"]
                    },
                    new ForeignKeyNeighborDto
                    {
                        DirectionCode = ForeignKeyDirectionCodes.Outgoing,
                        ConstraintName = "fk_user_role",
                        SchemaName = "identity",
                        TableName = "roles",
                        LocalColumns = ["role_id"],
                        NeighborColumns = ["id"]
                    }
                ],
                LintWarnings =
                [
                    new SchemaLintWarningDto
                    {
                        WarningCode = SchemaLintWarningCodes.GeneratedColumn,
                        ColumnName = "computed_total"
                    }
                ]
            }
        };
        var schemaService = new SchemaKnowledgeAppService(
            service,
            new SchemaKnowledgeManager(),
            new TableQueryDtoValidator());

        var result = await schemaService.DescribeTableAsync(
            new BridgeTableQueryDto
            {
                ConnectionId = Guid.NewGuid(),
                DbSchemaName = "identity",
                TableName = "users"
            },
            CancellationToken.None);

        result.DbSchemaName.ShouldBe("identity");
        result.PrimaryKey!.Columns.Single().ShouldBe("id");
        result.ForeignKeyNeighbors.Select(item => item.DirectionCode)
            .ShouldBe([PtnForeignKeyDirectionCodes.Outgoing, PtnForeignKeyDirectionCodes.Incoming]);
        result.ForeignKeyNeighbors.First().DbSchemaName.ShouldBe("identity");
        result.ForeignKeyNeighbors.Last().DbSchemaName.ShouldBe("support");
        result.LintWarnings.Single().WarningCode.ShouldBe(PtnSchemaLintWarningCodes.GeneratedColumn);
        result.LintWarnings.Single().ColumnName.ShouldBe("computed_total");
    }

    // Ayni semayi tablo ve kolon siralari ters iki checker snapshot'i olarak olusturur.
    private static SchemaSnapshotDto CreateSnapshot(bool reverse)
    {
        var first = new SchemaTableDto
        {
            Schema = "identity",
            Name = "users",
            Columns =
            [
                new SchemaColumnDto { Name = "email", Ordinal = reverse ? 2 : 2 },
                new SchemaColumnDto { Name = "id", Ordinal = reverse ? 1 : 1 }
            ]
        };
        var second = new SchemaTableDto
        {
            Schema = "identity",
            Name = "roles",
            Columns = [new SchemaColumnDto { Name = "id", Ordinal = 1 }]
        };
        return new SchemaSnapshotDto { Tables = reverse ? [first, second] : [second, first] };
    }

    // islevi: Checker default-interface metotlarini gercek imzalarla uygulayan test sema kaynagidir.
    // sistemdeki gorevi: Adapter testlerini proxy kutuphanesinin default-method sinirindan bagimsiz tutar.
    private sealed class SchemaDiscoveryFake : ISchemaDiscoveryAppService
    {
        private readonly IReadOnlyList<SchemaSnapshotDto> _snapshots;
        private int _snapshotIndex;

        // Adapter cagrilarinda sirayla dondurulecek sema fotograflarini kaydeder.
        public SchemaDiscoveryFake(params SchemaSnapshotDto[] snapshots)
        {
            _snapshots = snapshots;
        }

        public TableDescriptionDto TableDescription { get; set; } = new();

        // Bu test diliminde kullanilmayan sema listesini bos dondurur.
        public Task<List<DatabaseSchemaDto>> GetSchemasAsync(System.Guid connectionId)
        {
            return Task.FromResult(new List<DatabaseSchemaDto>());
        }

        // Bu test diliminde kullanilmayan nesne listesini bos dondurur.
        public Task<List<DatabaseSchemaObjectDto>> GetObjectsAsync(System.Guid connectionId, string schema)
        {
            return Task.FromResult(new List<DatabaseSchemaObjectDto>());
        }

        // Siradaki sema snapshot'ini default cancellation overload'u icin dondurur.
        public Task<SchemaSnapshotDto> GetSnapshotAsync(
            System.Guid connectionId,
            List<string> schemaNames)
        {
            return Task.FromResult(NextSnapshot());
        }

        // Siradaki sema snapshot'ini cancellation-aware adapter cagrisi icin dondurur.
        public Task<SchemaSnapshotDto> GetSnapshotAsync(
            System.Guid connectionId,
            List<string> schemaNames,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(NextSnapshot());
        }

        // Testin ayarladigi tek tablo ozetini adapter'a dondurur.
        public Task<TableDescriptionDto> DescribeTableAsync(
            System.Guid connectionId,
            string schema,
            string table,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(TableDescription);
        }

        // Cagri sayisina gore ayarlanan snapshot'i sirayla secer.
        private SchemaSnapshotDto NextSnapshot()
        {
            var index = System.Math.Min(_snapshotIndex, _snapshots.Count - 1);
            _snapshotIndex++;
            return _snapshots[index];
        }
    }
}
