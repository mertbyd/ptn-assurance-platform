using System;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Dtos.Connections;

namespace Ptn.DatabaseChecker.Services.Connections;

// islevi: Veritabani baglantisi temel CRUD servis kontratini tanimlar.
// sistemdeki gorevi: Baglanti adres defteri islemlerini controllerdan bagimsiz Application kontrati olarak sunar.
public interface IDatabaseConnectionAppService : IEntityCrudAppService<DatabaseConnectionDto, CreateDatabaseConnectionDto, UpdateDatabaseConnectionDto>
{
    // islevi: Baglantiyi emekli eder (pasife ceker); silme yerine IsActive=false, gecmis Run FK'lari korunur.
    Task<DatabaseConnectionDto> PassivateAsync(Guid id);

    // islevi: Baglantiya gercekten baglanmayi dener; kimligi Vault'tan cozer, motora uygun surucuyle test eder, sonucu (basari/hata + surum) doner. Sifre asla cevaba girmez.
    Task<TestConnectionResultDto> TestConnectionAsync(Guid id);
}
