using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Application.Mappers.SchemaDiscovery;
using Ptn.DatabaseChecker.Dtos.SchemaDiscovery;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Services.SchemaDiscovery;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.Application.Services.SchemaDiscovery;

// islevi: Kayitli bir baglanti uzerinden hedef veritabani sema kesfi use-case'lerini yonetir (schemas/objects/snapshot).
// sistemdeki gorevi: Baglanti CRUD adres defterinden ayri, ince orkestrasyon: entity oku -> manager -> Mapperly DTO. Secret cozumleme + motor secimi manager'da kalir; sifre cevaba/log'a girmez.
[RemoteService(IsEnabled = false)]
[UnitOfWork(IsDisabled = true)]
public class SchemaDiscoveryAppService : DatabaseCheckerAppService, ISchemaDiscoveryAppService
{
    // Mapperly source-generated mapper; stateless oldugu icin tek statik ornek yeterli.
    private static readonly SchemaDiscoveryMapper Mapper = new();

    // Kesif ara akisi (secret + motor + katalog) domain servisinde isletilir.
    private SchemaDiscoveryManager Manager => LazyServiceProvider.LazyGetRequiredService<SchemaDiscoveryManager>();

    // Baglanti Engine navigation'iyla okunur; motor kodu okuyucu secimi icin gereklidir.
    private IDatabaseConnectionRepository ConnectionRepository => LazyServiceProvider.LazyGetRequiredService<IDatabaseConnectionRepository>();

    // islevi: 0.1.x ikili uyumlu kesif cagrilarini token-aware akisa yonlendirir.
    public Task<List<DatabaseSchemaDto>> GetSchemasAsync(Guid connectionId)
        => GetSchemasAsync(connectionId, default);

    // islevi: Baglantidaki kullanici semalarini hafif listeler (kesif 1. adim).
    public async Task<List<DatabaseSchemaDto>> GetSchemasAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(connectionId, cancellationToken);
        var schemas = await Manager.GetSchemasAsync(connection, cancellationToken);
        return Mapper.MapToSchemaDtos(schemas);
    }

    // islevi: Verilen semadaki nesneleri (tablo/view/trigger/function/procedure) hafif listeler (kesif 2. adim).
    public async Task<List<DatabaseSchemaObjectDto>> GetObjectsAsync(
        Guid connectionId,
        string schema,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(connectionId, cancellationToken);
        var objects = await Manager.GetObjectsAsync(connection, schema, cancellationToken);
        return Mapper.MapToObjectDtos(objects);
    }

    // islevi: 0.1.x ikili uyumlu nesne listesini token-aware akisa yonlendirir.
    public Task<List<DatabaseSchemaObjectDto>> GetObjectsAsync(Guid connectionId, string schema)
        => GetObjectsAsync(connectionId, schema, default);

    // islevi: Verilen semalarin (bos ise tum kullanici semalari) tam sema fotografini dondurur.
    public async Task<SchemaSnapshotDto> GetSnapshotAsync(
        Guid connectionId,
        List<string> schemaNames,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(connectionId, cancellationToken);
        var snapshot = await Manager.ReadSnapshotAsync(
            connection, schemaNames ?? new List<string>(), cancellationToken);
        return Mapper.MapToSnapshotDto(snapshot);
    }

    // islevi: 0.1.x ikili uyumlu snapshot cagrilarini token-aware akisa yonlendirir.
    public Task<SchemaSnapshotDto> GetSnapshotAsync(Guid connectionId, List<string> schemaNames)
        => GetSnapshotAsync(connectionId, schemaNames, default);

    // islevi: Verilen semalarin yapisini sema fotografi dondurmeden tek kanonik muhre ve dal muhurlerine indirger.
    public async Task<SchemaFingerprintDto> GetSchemaFingerprintAsync(
        Guid connectionId,
        List<string> schemaNames,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(connectionId, cancellationToken);
        var fingerprint = await Manager.ReadFingerprintAsync(
            connection, schemaNames ?? new List<string>(), cancellationToken);
        return Mapper.MapToFingerprintDto(fingerprint);
    }

    // islevi: MALIYETLI; yalniz senaryo YAZIM aninda full snapshot'i tek tablo ve FK komsuluk ozetine indirger.
    public async Task<TableDescriptionDto> DescribeTableAsync(
        Guid connectionId,
        string schema,
        string table,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(connectionId, cancellationToken);
        var description = await Manager.DescribeTableAsync(
            connection, schema, table, cancellationToken);
        return Mapper.MapToTableDescriptionDto(description);
    }
}
