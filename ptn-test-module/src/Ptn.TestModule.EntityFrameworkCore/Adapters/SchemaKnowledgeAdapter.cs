using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Dtos.SchemaDiscovery;
using Ptn.DatabaseChecker.Services.SchemaDiscovery;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Adapters;

// islevi: Database checker sema DTO'larini kopru modellerine cevirir ve kanonik SHA-256 fingerprint hesaplar.
// sistemdeki gorevi: Profil sema muhrunu provider sirasi ve checker transport tiplerinden bagimsiz tutar.
public class SchemaKnowledgeAdapter : ISchemaKnowledgePort
{
    private readonly ISchemaDiscoveryAppService _appService;

    // Database checker public sema AppService'ini yalniz anti-corruption adapter'ina baglar.
    public SchemaKnowledgeAdapter(ISchemaDiscoveryAppService appService)
    {
        _appService = appService;
    }

    // Tek tablo adresini checker'a sorup kolon ve anahtarlari domain modeline cevirir.
    public async Task<PtnTableDescription> DescribeTableAsync(
        PtnTableQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _appService.DescribeTableAsync(
            query.ConnectionId,
            query.DbSchemaName,
            query.TableName,
            cancellationToken);
        return MapTableDescription(result);
    }

    // Tum kullanici semalarini checker'dan alip kanonik tablo/kolon fotografina cevirir.
    public async Task<PtnSchemaSnapshot> GetSnapshotAsync(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        var result = await _appService.GetSnapshotAsync(
            connectionId,
            new List<string>(),
            cancellationToken);
        return MapSnapshot(result);
    }

    // Kanonik siralanmis sema fotografini JSON'a cevirip sha256: fingerprint dondurur.
    public async Task<string> GetSchemaFingerprintAsync(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        var snapshot = await GetSnapshotAsync(connectionId, cancellationToken);
        var canonical = snapshot.Tables
            .OrderBy(table => table.DbSchemaName, StringComparer.Ordinal)
            .ThenBy(table => table.TableName, StringComparer.Ordinal)
            .Select(table => new
            {
                table.DbSchemaName,
                table.TableName,
                Columns = table.Columns.Order(StringComparer.Ordinal).ToArray()
            });
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(canonical));
        return PtnBridgeSettingNames.FingerprintPrefix +
               Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    // Checker tablo ozetini API/DB sema adlarini ayiran kopru modeline cevirir.
    private static PtnTableDescription MapTableDescription(TableDescriptionDto result)
    {
        var keys = result.UniqueIndexes.Select(key => MapKey(key, PtnTableKeyKindCodes.Unique)).ToList();
        if (result.PrimaryKey is not null)
        {
            keys.Insert(0, MapKey(result.PrimaryKey, PtnTableKeyKindCodes.Primary));
        }

        return new PtnTableDescription
        {
            Location = new PtnLocation
            {
                DbSchemaName = result.SchemaName,
                DbTableName = result.TableName
            },
            Columns = result.Columns.Select(MapColumn).ToList(),
            Keys = keys
        };
    }

    // Checker tablo kolonu ozetini provider-bagimsiz kopru kolonuna cevirir.
    private static PtnTableDescription.PtnTableColumn MapColumn(TableDescriptionColumnDto column)
    {
        return new PtnTableDescription.PtnTableColumn
        {
            Name = column.Name,
            DataTypeCode = column.CanonicalDataTypeCode,
            IsNullable = column.IsNullable
        };
    }

    // Checker PK veya unique index tanimini kapali anahtar turu ve kolon listesine cevirir.
    private static PtnTableDescription.PtnTableKey MapKey(TableKeyDefinitionDto key, string kindCode)
    {
        return new PtnTableDescription.PtnTableKey
        {
            KindCode = kindCode,
            Columns = key.Columns
        };
    }

    // Checker full snapshot'ini fingerprint icin gereken minimum kanonik tablo/kolon sekline cevirir.
    private static PtnSchemaSnapshot MapSnapshot(SchemaSnapshotDto result)
    {
        return new PtnSchemaSnapshot
        {
            Tables = result.Tables.Select(table => new PtnSchemaSnapshot.PtnSchemaTable
            {
                DbSchemaName = table.Schema,
                TableName = table.Name,
                Columns = table.Columns.OrderBy(column => column.Ordinal).Select(column => column.Name).ToList()
            }).ToList()
        };
    }
}
