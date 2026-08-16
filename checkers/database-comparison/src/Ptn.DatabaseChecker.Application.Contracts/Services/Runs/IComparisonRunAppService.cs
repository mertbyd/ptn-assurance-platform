using System;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Dtos.Reports;
using Ptn.DatabaseChecker.Dtos.Runs;
using Ptn.DatabaseChecker.Dtos.Findings;
using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Services.Runs;

// islevi: Karsilastirma run kaydi icin okuma, detay, calistir-ve-sakla ve rapor servis kontratini tanimlar.
// sistemdeki gorevi: Run tarihcesi (hafif liste/get), agir detay (findings/reports/scope), bir tarifi calistirip kalici run uretme ve yapisal rapor (ozet + gruplama) akislarini servis katmanina tasir. Guncelleme/silme acilmaz (tarih degistirilemez).
public interface IComparisonRunAppService : IEntityCreateAppService<ComparisonRunDto, CreateComparisonRunDto>
{
    /// <summary>
    /// Run'i tum bulgular ve rapor icerikleriyle (agir owned veri) getirir; scope kurallari response'a dahil edilmez.
    /// </summary>
    /// <param name="id">Detayi okunacak run'in kimligi.</param>
    /// <returns>Run'in agir detay cevap modeli.</returns>
    Task<ComparisonRunDetailDto> GetDetailAsync(Guid id);

    /// <summary>
    /// Bir tarifi fiilen calistirir (iki baglantiyi moda gore kiyaslar) ve sonucu kalici run olarak saklar.
    /// </summary>
    /// <param name="input">Calistirilacak tarifin kimligini tasiyan istek.</param>
    /// <returns>Olusan run'in agir detay cevap modeli (findings/kapsam dahil).</returns>
    Task<ComparisonRunDetailDto> ExecuteAsync(ExecuteComparisonRunDto input);

    /// <summary>
    /// Run'in yapisal raporunu (ozet + tur/tablo gruplama + detay findings) uretir.
    /// </summary>
    /// <param name="id">Raporu istenen run'in kimligi.</param>
    /// <returns>Run'in ozet ve gruplu rapor cevap modeli.</returns>
    Task<ComparisonReportDto> GetReportAsync(Guid id);

    /// <summary>
    /// Run bulgularini sunucu tarafli filtre ve ABP sayfalama ile kucuk parcalar halinde okur.
    /// </summary>
    /// <param name="id">Bulgulari okunacak run kimligi.</param>
    /// <param name="input">Siddet/tur/adres filtreleri ve ABP sayfalama girdisi.</param>
    /// <param name="cancellationToken">Istek iptal edildiginde owned JSON sorgularini durduran token.</param>
    /// <returns>Filtreli toplam sayi ve 32 KB butceye sigan bulgu sayfasi.</returns>
    Task<PagedResultDto<FindingDto>> GetFindingsAsync(
        Guid id,
        FindingQueryInput input,
        CancellationToken cancellationToken)
        => Task.FromException<PagedResultDto<FindingDto>>(new NotSupportedException());
}
