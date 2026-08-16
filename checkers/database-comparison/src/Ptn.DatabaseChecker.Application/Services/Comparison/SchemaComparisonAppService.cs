using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Application.Mappers.Comparison;
using Ptn.DatabaseChecker.Dtos.Comparison;
using Ptn.DatabaseChecker.Dtos.Findings;
using Ptn.DatabaseChecker.Dtos.Scopes;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Services.Comparison;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.Application.Services.Comparison;

// islevi: Iki baglanti arasinda sema karsilastirma use-case'ini yonetir (kaynak/hedef oku -> motor -> bulgular).
// sistemdeki gorevi: Kesif akisindan ayri ince orkestrasyon: iki baglantiyi Engine detayiyla oku -> kapsam kurallarini modele tasi -> execution manager -> Mapperly findings DTO. Secret cozumleme + motor secimi kesif/manager katmaninda kalir, sifre cevaba girmez; sonuc persist edilmez.
[RemoteService(IsEnabled = false)]
[UnitOfWork(IsDisabled = true)]
public class SchemaComparisonAppService : DatabaseCheckerAppService, ISchemaComparisonAppService
{
    // Mapperly source-generated mapper; stateless oldugu icin tek statik ornek yeterli.
    private static readonly SchemaComparisonMapper Mapper = new();
    // Iki snapshot okuma + motor calistirma orkestrasyonu domain servisinde isletilir.
    private SchemaComparisonExecutionManager Manager
        => LazyServiceProvider.LazyGetRequiredService<SchemaComparisonExecutionManager>();
    // Baglantilar Engine navigation'iyla okunur; motor kodu okuyucu secimi icin gereklidir.
    private IDatabaseConnectionRepository ConnectionRepository
        => LazyServiceProvider.LazyGetRequiredService<IDatabaseConnectionRepository>();
    // islevi: Kaynak ve hedef baglantiyi secilen moda (SchemaOnly/DataOnly/Both) ve kapsam filtresine gore kiyaslar; moda gore yapisal ve/veya veri bulgularini doner.
    public async Task<ComparisonFindingsDto> CompareSchemaAsync(CompareSchemaRequestDto input)
    {
        var sourceConnection = await ConnectionRepository.GetWithDetailsAsync(input.SourceConnectionId);
        var targetConnection = await ConnectionRepository.GetWithDetailsAsync(input.TargetConnectionId);
        var scopeRules = Mapper.MapToScopeRules(input.ScopeRules ?? new List<ScopeRuleDto>());
        var findings = await Manager.CompareSchemasAsync(
            sourceConnection,
            targetConnection,
            input.SchemaNames ?? new List<string>(),
            scopeRules,
            input.ComparisonTypeCode);
        return Mapper.MapToFindingsDto(findings);
    }
}
