using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Scopes;

namespace Ptn.DatabaseChecker.Dtos.Comparison;

// islevi: Iki baglanti arasinda sema karsilastirma istegini (kaynak/hedef + kapsam filtresi) tasir.
// sistemdeki gorevi: SchemaComparisonController POST govdesi; ScopeRules ile kullanici hangi sema/nesnelerin kiyaslanacagini (Include/Exclude/Ignore) daraltir, SchemaNames ile hangi semalarin okunacagini sinirlar.
public class CompareSchemaRequestDto
{
    /// <summary>
    /// Kaynak (ornegin Test) veritabani baglantisinin kimligi.
    /// </summary>
    public Guid SourceConnectionId { get; set; }

    /// <summary>
    /// Hedef (ornegin Canli) veritabani baglantisinin kimligi.
    /// </summary>
    public Guid TargetConnectionId { get; set; }

    /// <summary>
    /// Anlik karsilastirmanin modu (ComparisonTypeCodes.*): SchemaOnly / DataOnly / Both. Motor bu koda gore yapisal ve/veya veri bloklarini calistirir.
    /// </summary>
    public string ComparisonTypeCode { get; set; } = default!;

    /// <summary>
    /// Okunacak sema adlari; bos birakilirsa iki tarafta da tum kullanici semalari okunur.
    /// </summary>
    public List<string>? SchemaNames { get; set; }

    /// <summary>
    /// Kapsam kurallari; Include/Exclude/Ignore ile karsilastirmaya girecek sema/nesneler daraltilir. Bos ise tum okunan nesneler kiyaslanir.
    /// </summary>
    public List<ScopeRuleDto>? ScopeRules { get; set; }
}
