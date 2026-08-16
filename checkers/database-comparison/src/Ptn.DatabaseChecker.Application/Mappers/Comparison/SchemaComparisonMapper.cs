using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Findings;
using Ptn.DatabaseChecker.Dtos.Scopes;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Comparison.Scope;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Comparison;

// islevi: Sema karsilastirma istek/sonuc modellerinin DTO donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Kapsam kurali DTO'sunu domain modeline, motor bulgularini API cevap DTO'suna elle kopyalamadan tasir.
[Mapper]
public partial class SchemaComparisonMapper
{
    // islevi: Istekteki kapsam kurallarini domain kapsam kurallarina tasir.
    public partial List<ComparisonScopeRule> MapToScopeRules(List<ScopeRuleDto> rules);

    // islevi: Motorun urettigi bulgulari API cevap DTO'suna tasir (sema/migration/veri).
    public partial ComparisonFindingsDto MapToFindingsDto(ComparisonFindings findings);
}
