using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Volo.Abp;
using CheckerForeignKeyDirectionCodes = Ptn.DatabaseChecker.Constants.Comparison.ForeignKeyDirectionCodes;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Database sema bilgisini Bridge anlamina tamamlar ve kanonik fingerprint uretir.
// sistemdeki gorevi: Sema semantigi ile kararli hash kuralini Application servisinden ayirir.
public class SchemaKnowledgeManager : TestModuleDomainService
{
    // Checker sema adini acik DbSchemaName alanina uygular.
    public TableDescription CreateDescription(CheckerTableDescription source)
    {
        return new TableDescription
        {
            DbSchemaName = source.SchemaName,
            TableName = source.TableName,
            Columns = source.Columns,
            PrimaryKey = source.PrimaryKey,
            UniqueIndexes = source.UniqueIndexes,
            ForeignKeyNeighbors = source.ForeignKeyNeighbors
                .Select(CreateNeighbor)
                .OrderBy(GetDirectionOrder)
                .ThenBy(item => item.DbSchemaName, StringComparer.Ordinal)
                .ThenBy(item => item.TableName, StringComparer.Ordinal)
                .ToList()
        };
    }

    // Checker FK komsusunu acik DB semasi ve Bridge yon koduyla tamamlar.
    private static ForeignKeyNeighbor CreateNeighbor(CheckerForeignKeyNeighbor source)
    {
        return new ForeignKeyNeighbor
        {
            DirectionCode = NormalizeDirection(source.DirectionCode),
            ConstraintName = source.ConstraintName,
            DbSchemaName = source.SchemaName,
            TableName = source.TableName,
            LocalColumns = source.LocalColumns,
            NeighborColumns = source.NeighborColumns
        };
    }

    // Giden baglari gelen baglardan once tutan kararli siralama anahtarini getirir.
    private static int GetDirectionOrder(ForeignKeyNeighbor neighbor) =>
        neighbor.DirectionCode == PtnForeignKeyDirectionCodes.Outgoing ? 0 : 1;

    // Checker FK yon kodunu Bridge'in kapali sozlugune cevirir.
    private static string NormalizeDirection(string directionCode) => directionCode switch
    {
        CheckerForeignKeyDirectionCodes.Outgoing => PtnForeignKeyDirectionCodes.Outgoing,
        CheckerForeignKeyDirectionCodes.Incoming => PtnForeignKeyDirectionCodes.Incoming,
        _ => throw new BusinessException(TestModuleBridgeErrorCodes.CheckerCallFailed)
            .WithData(nameof(directionCode), directionCode)
    };

    // Siralanmis sema fotografini JSON'a cevirip sha256 fingerprint dondurur.
    public string GetFingerprint(SchemaSnapshot snapshot)
    {
        var canonical = snapshot.Tables
            .OrderBy(table => table.Schema, StringComparer.Ordinal)
            .ThenBy(table => table.Name, StringComparer.Ordinal)
            .Select(table => new
            {
                table.Schema,
                table.Name,
                Columns = table.Columns.OrderBy(column => column.Ordinal).Select(column => column.Name).ToArray()
            });
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(canonical));
        return PtnBridgeSettingNames.FingerprintPrefix +
               Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
