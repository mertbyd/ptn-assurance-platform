using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Connections;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Connections;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Connections;

// islevi: Veritabani baglantisi DTO/model/entity donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Baglanti CRUD akisinda katmanlar arasi alan tasimayi tek noktada toplar.
[Mapper]
public partial class DatabaseConnectionMapper
{
    // islevi: Veritabani baglantisi entity bilgisini API cevap DTO'suna tasir.
    public partial DatabaseConnectionDto MapToDto(DatabaseConnection entity);

    // islevi: Veritabani baglantisi entity listesini API cevap DTO listesine tasir.
    public partial List<DatabaseConnectionDto> MapToDto(List<DatabaseConnection> entities);

    // islevi: Veritabani baglantisi olusturma istegini domain manager modeline tasir.
    // VaultSecretPath AppService'te Vault yazimindan sonra elle set edilir (source'ta yok); kimlik bilgileri Vault'a gider, modele/entity'ye tasinmaz.
    [MapperIgnoreTarget(nameof(CreateDatabaseConnectionModel.VaultSecretPath))]
    public partial CreateDatabaseConnectionModel MapToCreateModel(CreateDatabaseConnectionDto dto);

    // islevi: Veritabani baglantisi guncelleme istegini domain manager modeline tasir.
    // VaultSecretPath mevcut kayittan korunur (source'ta yok); kimlik bilgileri opsiyonel olarak Vault'a yazilir, modele/entity'ye tasinmaz.
    [MapperIgnoreTarget(nameof(UpdateDatabaseConnectionModel.VaultSecretPath))]
    public partial UpdateDatabaseConnectionModel MapToUpdateModel(UpdateDatabaseConnectionDto dto);

    // islevi: Manager tarafindan dogrulanmis baglanti olusturma modelini yeni entity uzerine uygular.
    [MapperIgnoreTarget(nameof(DatabaseConnection.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.CreationTime))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.CreatorId))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.Engine))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.LastModificationTime))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.LastModifierId))]
    public partial void MapToEntity(CreateDatabaseConnectionModel model, [MappingTarget] DatabaseConnection entity);

    // islevi: Manager tarafindan dogrulanmis baglanti guncelleme modelini mevcut entity uzerine uygular.
    [MapperIgnoreTarget(nameof(DatabaseConnection.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.CreationTime))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.CreatorId))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.Engine))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.LastModificationTime))]
    [MapperIgnoreTarget(nameof(DatabaseConnection.LastModifierId))]
    public partial void MapToEntity(UpdateDatabaseConnectionModel model, [MappingTarget] DatabaseConnection entity);

    // islevi: Baglanti testi sonucunu (domain modeli) API cevap DTO'suna tasir.
    public partial TestConnectionResultDto MapToTestResultDto(ConnectionTestResult result);
}
