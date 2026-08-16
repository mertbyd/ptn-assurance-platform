using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Connections;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.Connections;
using SystemStandards.Results;

namespace Ptn.DatabaseChecker.Controllers.Connections;

// islevi: Veritabani baglantisi okuma + yazma (create/update/passivate) endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Istekleri IDatabaseConnectionAppService'e yonlendiren ince controller; sifre alanlari yalnizca istek govdesinde gelir, cevapta donmez.
/// <summary>
/// Veritabani baglantisi islemleri.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.DatabaseConnections)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Connections)]
[Authorize(DatabaseCheckerPermissions.Connections.View)]
public class DatabaseConnectionController : EntityReadControllerBase<IDatabaseConnectionAppService, DatabaseConnectionDto>
{
    /// <summary>
    /// Yeni veritabani baglantisi olusturur; kimlik bilgisi Vault'a yazilir, DB'ye yalnizca secret path gider.
    /// </summary>
    /// <param name="input">Baglanti olusturma istegi (kullanici adi + sifre dahil).</param>
    /// <returns>Olusturulan baglantinin cevap modeli (sifre icermez).</returns>
    [HttpPost]
    [Authorize(DatabaseCheckerPermissions.Connections.Manage)]
    public async Task<Result<DatabaseConnectionDto>> Create([FromBody] CreateDatabaseConnectionDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }
    /// <summary>
    /// Mevcut baglantiyi gunceller; parola verilirse Vault'taki secret yeniden yazilir.
    /// </summary>
    /// <param name="id">Guncellenecek baglantinin kimligi.</param>
    /// <param name="input">Baglanti guncelleme istegi.</param>
    /// <returns>Guncellenen baglantinin cevap modeli (sifre icermez).</returns>
    [HttpPut(DatabaseCheckerHttpApiConstants.Segments.EntityById)]
    [Authorize(DatabaseCheckerPermissions.Connections.Manage)]
    public async Task<Result<DatabaseConnectionDto>> Update(Guid id, [FromBody] UpdateDatabaseConnectionDto input)
    {
        var result = await AppService.UpdateAsync(id, input);
        return result;
    }
    /// <summary>
    /// Baglantiyi emekli eder (pasife ceker); silmez, gecmis run kayitlari korunur.
    /// </summary>
    /// <param name="id">Pasife cekilecek baglantinin kimligi.</param>
    /// <returns>Guncellenen baglantinin cevap modeli.</returns>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.ConnectionPassivation)]
    [Authorize(DatabaseCheckerPermissions.Connections.Manage)]
    public async Task<Result<DatabaseConnectionDto>> Passivate(Guid id)
    {
        var result = await AppService.PassivateAsync(id);
        return result;
    }
    /// <summary>
    /// Baglantiya gercekten baglanmayi dener; kimligi Vault'tan cozer, motora uygun surucuyle test eder.
    /// </summary>
    /// <param name="id">Test edilecek baglantinin kimligi.</param>
    /// <returns>Erisim durumu, sunucu surumu ve varsa hata mesaji (sifre icermez).</returns>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.ConnectionTest)]
    [Authorize(DatabaseCheckerPermissions.Connections.Manage)]
    public async Task<Result<TestConnectionResultDto>> TestConnection(Guid id)
    {
        var result = await AppService.TestConnectionAsync(id);
        return result;
    }
}
