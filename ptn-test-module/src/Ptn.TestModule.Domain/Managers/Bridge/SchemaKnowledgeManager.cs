using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Database sema bilgisini Bridge anlamina tamamlar ve kanonik fingerprint uretir.
// sistemdeki gorevi: Sema semantigi ile kararli hash kuralini Application servisinden ayirir.
public class SchemaKnowledgeManager : TestModuleDomainService
{
    // Checker sema adini acik DbSchemaName alanina uygular.
    public PtnTableDescription CreateDescription(PtnCheckerTableDescription source)
    {
        return new PtnTableDescription
        {
            DbSchemaName = source.SchemaName,
            TableName = source.TableName,
            Columns = source.Columns,
            PrimaryKey = source.PrimaryKey,
            UniqueIndexes = source.UniqueIndexes
        };
    }

    // Siralanmis sema fotografini JSON'a cevirip sha256 fingerprint dondurur.
    public string GetFingerprint(PtnSchemaSnapshot snapshot)
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
