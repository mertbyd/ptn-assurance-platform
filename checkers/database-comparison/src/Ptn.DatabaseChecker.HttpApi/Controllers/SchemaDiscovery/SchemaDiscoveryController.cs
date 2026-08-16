using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.SchemaDiscovery;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.SchemaDiscovery;
using SystemStandards.Results;
namespace Ptn.DatabaseChecker.Controllers.SchemaDiscovery;
// islevi: Kayitli bir baglanti uzerinden hedef veritabani sema kesfi (schemas/objects/snapshot) endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Istekleri ISchemaDiscoveryAppService'e yonlendiren ince controller; baglanti CRUD adres defterinden ayri kesif yuzeyidir, sifre alanlari cevaba girmez.
/// <summary>
/// Veritabani sema kesif islemleri.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.SchemaDiscovery)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.SchemaDiscovery)]
[Authorize(DatabaseCheckerPermissions.Connections.View)]
public class SchemaDiscoveryController : DatabaseCheckerController
{
    private ISchemaDiscoveryAppService AppService => LazyServiceProvider.LazyGetRequiredService<ISchemaDiscoveryAppService>();

    [NonAction]
    public Task<Result<List<DatabaseSchemaDto>>> GetSchemas(Guid connectionId)
        => GetSchemas(connectionId, default);
    /// <summary>
    /// Baglantidaki kullanici semalarini listeler (sistem/rol semalari haric). Kesif akisinin ilk adimidir.
    /// </summary>
    /// <param name="connectionId">Kesif yapilacak baglantinin kimligi.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde katalog okumasini durduran token.</param>
    /// <returns>Kullanici sema adlari (sifre icermez).</returns>
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.ConnectionSchemas)]
    public async Task<Result<List<DatabaseSchemaDto>>> GetSchemas(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        var result = await AppService.GetSchemasAsync(connectionId, cancellationToken);
        return result;
    }
    /// <summary>
    /// Verilen semadaki nesneleri (tablo/view/trigger/function/procedure) listeler. Kesif akisinin ikinci adimidir.
    /// </summary>
    /// <param name="connectionId">Kesif yapilacak baglantinin kimligi.</param>
    /// <param name="schema">Nesneleri listelenecek sema adi.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde katalog okumasini durduran token.</param>
    /// <returns>Nesne adi + kararli tur kodu listesi (sifre icermez).</returns>
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.ConnectionObjects)]
    public async Task<Result<List<DatabaseSchemaObjectDto>>> GetObjects(
        Guid connectionId,
        [FromQuery] string schema,
        CancellationToken cancellationToken)
    {
        var result = await AppService.GetObjectsAsync(connectionId, schema, cancellationToken);
        return result;
    }

    [NonAction]
    public Task<Result<List<DatabaseSchemaObjectDto>>> GetObjects(Guid connectionId, string schema)
        => GetObjects(connectionId, schema, default);
    /// <summary>
    /// Verilen semalarin (bos birakilirsa tum kullanici semalari) tam sema fotografini (tablo + kolon detaylari) dondurur.
    /// </summary>
    /// <param name="connectionId">Kesif yapilacak baglantinin kimligi.</param>
    /// <param name="schemaNames">Fotografi cekilecek sema adlari; bos birakilirsa tum kullanici semalari okunur.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde katalog okumasini durduran token.</param>
    /// <returns>Motor kodu, veritabani adi ve tablolari kolonlariyla iceren sema fotografisi (sifre icermez).</returns>
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.ConnectionSnapshot)]
    public async Task<Result<SchemaSnapshotDto>> GetSnapshot(
        Guid connectionId,
        [FromQuery] List<string> schemaNames,
        CancellationToken cancellationToken)
    {
        var result = await AppService.GetSnapshotAsync(connectionId, schemaNames, cancellationToken);
        return result;
    }

    [NonAction]
    public Task<Result<SchemaSnapshotDto>> GetSnapshot(Guid connectionId, List<string> schemaNames)
        => GetSnapshot(connectionId, schemaNames, default);
    // islevi: Sema fotografini dondurmeden hedefin kanonik yapi muhrunu mevcut schema-discovery transport yuzeyinde acar.
    /// <summary>
    /// Verilen semalarin (bos birakilirsa tum kullanici semalari) kanonik yapi muhrunu dondurur; fotograf ne dondurulur ne de saklanir.
    /// </summary>
    /// <param name="connectionId">Muhru hesaplanacak baglantinin kimligi.</param>
    /// <param name="schemaNames">Muhre girecek sema adlari; bos birakilirsa tum kullanici semalari okunur.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde katalog okumasini durduran token.</param>
    /// <returns>Snapshot muhru, algoritma kimligi ve sema/tablo dal muhurleri (sifre icermez).</returns>
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.ConnectionSchemaFingerprint)]
    public async Task<Result<SchemaFingerprintDto>> GetSchemaFingerprint(
        Guid connectionId,
        [FromQuery] List<string> schemaNames,
        CancellationToken cancellationToken)
    {
        var result = await AppService.GetSchemaFingerprintAsync(connectionId, schemaNames, cancellationToken);
        return result;
    }
    // islevi: Tek tablonun assertion odakli tanimini mevcut schema-discovery transport yuzeyinde acar.
    /// <summary>
    /// Maliyetli tablo tanimlamasini yalniz senaryo yaziminda kullanir; kolon, PK, unique index ve bir-seviye FK komsularini dondurur.
    /// </summary>
    /// <param name="connectionId">Kesif yapilacak baglantinin kimligi.</param>
    /// <param name="schema">Tanimlanacak tablonun semasi.</param>
    /// <param name="table">Tanimlanacak tablonun adi.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde hedefli katalog okumasini durduran token.</param>
    /// <returns>Assertion yazimi icin kolon, anahtar ve FK komsuluk ozeti.</returns>
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.TableDescription)]
    public async Task<Result<TableDescriptionDto>> DescribeTable(
        Guid connectionId,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        var result = await AppService.DescribeTableAsync(
            connectionId, schema, table, cancellationToken);
        return result;
    }
}
