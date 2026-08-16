using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.Models.Secrets;
using Ptn.ApiContractChecker.Models.Sources;
using Riok.Mapperly.Abstractions;

namespace Ptn.ApiContractChecker.Application.Mappers.Sources;

// islevi: SpecSource entity, aggregate dokumani, domain model ve DTO donusumlerini uretir.
// sistemdeki gorevi: Secret alanlarini response sozlesmesine tasimadan her eslemenin tek sahibi olur.
[Mapper]
public partial class SpecSourceMapper
{
    // Kaynak aggregate'ini secret tasimayan API yanitina cevirir.
    public partial SpecSourceDto MapToDto(SpecSource source);

    // Kaynak listesini ayni secret-tasimayan yanit sozlesmesine cevirir.
    public partial List<SpecSourceDto> MapToDto(List<SpecSource> sources);

    // Aggregate dokumanini dis okuma sozlesmesine cevirir.
    public partial SpecDocumentDto MapToDto(SpecDocument document);

    // Izleme istegini aggregate davranisina verilecek domain modeline tasir.
    public partial SpecDocumentMonitoringModel MapToMonitoringModel(ConfigureSpecDocumentMonitoringDto input);

    // Guncellenen dokumani yalniz izleme alanlarini tasiyan yanit DTO'suna cevirir.
    public partial SpecDocumentMonitoringDto MapToMonitoringDto(SpecDocument document);

    // Credential degeri AppService/Vault akisi tarafindan tamamlanacak create modelini uretir.
    [MapperIgnoreTarget(nameof(CreateSpecSourceModel.VaultSecretPath))]
    public partial CreateSpecSourceModel MapToCreateModel(CreateSpecSourceDto input);

    // Mevcut veya yeni Vault yolu AppService tarafindan tamamlanacak update modelini uretir.
    [MapperIgnoreTarget(nameof(UpdateSpecSourceModel.VaultSecretPath))]
    public partial UpdateSpecSourceModel MapToUpdateModel(UpdateSpecSourceDto input);

    // Credential ciftini Vault'a yazilacak secret modeline cevirir.
    public partial ApiCredentialModel MapToCredential(CreateSpecSourceDto input);

    // Erisilebilirlik sonucunu secret veya transport ayrintisi tasimayan API yanitina cevirir.
    public partial SpecSourceReachabilityDto MapToReachabilityDto(SpecSourceReachabilityModel model);
}
