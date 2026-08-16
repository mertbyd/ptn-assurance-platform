using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Dtos.SchemaDiscovery;
using Volo.Abp.Application.Services;

namespace Ptn.DatabaseChecker.Services.SchemaDiscovery;

// islevi: Kayitli bir baglanti uzerinden hedef veritabani sema kesfi (schemas/objects/snapshot) servis kontratini tanimlar.
// sistemdeki gorevi: Kesif use-case'lerini baglanti CRUD adres defterinden ayri, controllerdan bagimsiz Application kontrati olarak sunar.
public interface ISchemaDiscoveryAppService : IApplicationService
{
    Task<List<DatabaseSchemaDto>> GetSchemasAsync(Guid connectionId);

    // islevi: Baglantidaki kullanici semalarini hafif listeler (sistem/rol semalari haric); kesif akisinin 1. adimi.
    Task<List<DatabaseSchemaDto>> GetSchemasAsync(
        Guid connectionId,
        CancellationToken cancellationToken)
        => GetSchemasAsync(connectionId);

    Task<List<DatabaseSchemaObjectDto>> GetObjectsAsync(Guid connectionId, string schema);

    // islevi: Verilen semadaki nesneleri (tablo/view/trigger/function/procedure) hafif listeler; kesif akisinin 2. adimi.
    Task<List<DatabaseSchemaObjectDto>> GetObjectsAsync(
        Guid connectionId,
        string schema,
        CancellationToken cancellationToken)
        => GetObjectsAsync(connectionId, schema);

    Task<SchemaSnapshotDto> GetSnapshotAsync(Guid connectionId, List<string> schemaNames);

    // islevi: Verilen semalarin (bos ise tum kullanici semalari) tam sema fotografini (tablo + kolon detaylari) dondurur.
    Task<SchemaSnapshotDto> GetSnapshotAsync(
        Guid connectionId,
        List<string> schemaNames,
        CancellationToken cancellationToken)
        => GetSnapshotAsync(connectionId, schemaNames);

    // islevi: Verilen semalarin (bos ise tum kullanici semalari) yapisini sema fotografi dondurmeden tek kanonik muhre ve dal muhurlerine indirger.
    Task<SchemaFingerprintDto> GetSchemaFingerprintAsync(
        Guid connectionId,
        List<string> schemaNames,
        CancellationToken cancellationToken)
        => Task.FromException<SchemaFingerprintDto>(new NotSupportedException());

    // islevi: MALIYETLI; yalniz senaryo YAZIM aninda tek tablonun assertion odakli yapisini ve bir-seviye FK komsularini tanimlar.
    Task<TableDescriptionDto> DescribeTableAsync(
        Guid connectionId,
        string schema,
        string table,
        CancellationToken cancellationToken)
        => Task.FromException<TableDescriptionDto>(new NotSupportedException());
}
