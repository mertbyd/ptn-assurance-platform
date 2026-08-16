using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Scopes;

namespace Ptn.DatabaseChecker.Dtos.Runs;

// islevi: Bir tarifi calistirip sonucu kalici run olarak saklama istegidir.
// sistemdeki gorevi: Run bir tariften dogar; kaynak/hedef/mod tariften, kalici olmayan kapsam kurallari bu calistirma isteginden okunur; sonuc, sayaclar ve durum motor tarafindan yazilir.
public class ExecuteComparisonRunDto
{
    /// <summary>
    /// Calistirilacak karsilastirma tarifinin kimligi.
    /// </summary>
    public Guid ComparisonDefinitionId { get; set; }

    /// <summary>
    /// Yalniz bu calistirmada uygulanacak kapsam kurallari; null veya bos liste kisit yok anlamina gelir ve definition'a kaydedilmez.
    /// </summary>
    public List<ScopeRuleDto>? ScopeRules { get; set; }
}
