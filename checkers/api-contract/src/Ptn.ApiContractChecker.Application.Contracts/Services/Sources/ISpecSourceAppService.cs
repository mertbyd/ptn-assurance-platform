using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.Dtos.Sources;

namespace Ptn.ApiContractChecker.Services.Sources;

// islevi: SpecSource okuma, create, update, pasiflestirme ve erisilebilirlik kullanim senaryolarini tanimlar.
// sistemdeki gorevi: SpecDocument'i ayri servis yapmadan aggregate root API sozlesmesini controller'dan bagimsiz sunar.
public interface ISpecSourceAppService : IEntityReadAppService<SpecSourceDto>
{
    // Kaynagi dokumanlariyla kurar ve varsa kimlik ciftini once Vault'a yazar.
    Task<SpecSourceDto> CreateAsync(CreateSpecSourceDto input);

    // Kaynak ve dokumanlarini gunceller; kimlik cifti yoksa mevcut secret'i korur.
    Task<SpecSourceDto> UpdateAsync(Guid id, UpdateSpecSourceDto input);

    // Kaynagi fiziksel silmeden pasiflestirir.
    Task<SpecSourceDto> PassivateAsync(Guid id);

    // Kaynagin aktif dokumanlarina secretsiz erisilebilirlik testi yapar.
    Task<SpecSourceReachabilityDto> TestReachabilityAsync(Guid id);

    // Tek aktif dokumanin canli govdesini cekip snapshot karar akisini isletir.
    Task<SpecSnapshotDto> CaptureSnapshotAsync(Guid id, Guid documentId);

    // Tek aktif dokumanin zamanlanmis izleme tercihini ve kontrol araligini gunceller.
    Task<SpecDocumentMonitoringDto> ConfigureDocumentMonitoringAsync(
        Guid id,
        Guid documentId,
        ConfigureSpecDocumentMonitoringDto input);
}
