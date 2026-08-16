using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Scope;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Kapsam kurallarinin tablo/nesne dahil etme, haric tutma ve veri-sayimi secimini yorumlar.
// sistemdeki gorevi: T6/T7 motoru discovery snapshot'ini islerken ScopeRules semantigini tek merkezden uygular; Exclude/Ignore her zaman Include'dan once kazanir.
public class ComparisonScopeRuleEvaluator : DomainService
{
    // islevi: Snapshot tablo listesinden sema/tablo karsilastirmasina girecek tablolari secili kapsam kurallarina gore filtreler.
    public List<SchemaTableModel> FilterComparableTables(
        List<SchemaTableModel> tables,
        List<ComparisonScopeRule> scopeRules)
    {
        return tables
            .Where(table => ShouldCompareObject(scopeRules, table.Schema, table.Name))
            .ToList();
    }

    // islevi: Snapshot tablo disi nesne listesinden sema karsilastirmasina girecek nesneleri kapsam kurallarina gore filtreler.
    public List<SchemaObjectDefinitionModel> FilterComparableObjects(
        List<SchemaObjectDefinitionModel> objects,
        List<ComparisonScopeRule> scopeRules)
    {
        return objects
            .Where(schemaObject => ShouldCompareObject(scopeRules, schemaObject.Schema, schemaObject.Name))
            .ToList();
    }

    // islevi: Snapshot tablo listesinden satir sayisi/veri karsilastirmasina girecek secili tablolari filtreler.
    public List<SchemaTableModel> FilterDataCompareTables(
        List<SchemaTableModel> tables,
        List<ComparisonScopeRule> scopeRules)
    {
        return tables
            .Where(table => ShouldCompareDataForTable(scopeRules, table.Schema, table.Name))
            .ToList();
    }

    // islevi: DataCompare kurallarindan discovery snapshot'i okumadan dogrudan exact sema+tablo okuma plani uretir.
    // sistemdeki gorevi: DataOnly akisinin iki tam schema snapshot'ina baglanmasini engeller; Exclude/Ignore/Include semantigini ayni karar merkeziyle korur.
    public List<ComparisonTableIdentifierModel> BuildDataCompareTableIdentifiers(
        List<ComparisonScopeRule> scopeRules)
    {
        return GetRules(scopeRules, ScopeKindCodes.DataCompare)
            .Where(rule => !string.IsNullOrWhiteSpace(rule.ObjectName))
            .Where(rule => ShouldCompareDataForTable(scopeRules, rule.SchemaName, rule.ObjectName!))
            .GroupBy(
                rule => string.Join(
                    ComparisonCanonicalTextConstants.KeySeparator,
                    rule.SchemaName.Trim(),
                    rule.ObjectName!.Trim()),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(rule => rule.SchemaName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(rule => rule.ObjectName, StringComparer.OrdinalIgnoreCase)
            .Select(rule => new ComparisonTableIdentifierModel
            {
                SchemaName = rule.SchemaName,
                TableName = rule.ObjectName!
            })
            .ToList();
    }

    // islevi: Kapsama girmis bir tablonun alt nesnelerini (kolon/index/constraint/trigger) ChildName tasiyan kurallara gore daraltir.
    // sistemdeki gorevi: Include beyaz-liste, Exclude/Ignore kara-liste. Karar SIMETRIKTIR: whitelist bir tarafta (source/target) o child bulunsa da bulunmasa da uygulanir; boylece yalnizca bir tarafta olan bir kolon (yeni eklenmis/silinmis) "sadece secili child" semantigini bozmadan dogru OnlyInTarget/OnlyInSource olarak cikar - eksik taraf bosalir, karsi taraftaki child'i sahte "OnlyInSource" seline cevirmez.
    public List<TChild> FilterComparableChildren<TChild>(
        List<TChild> children,
        string schemaName,
        string objectName,
        Func<TChild, string> childNameSelector,
        List<ComparisonScopeRule> scopeRules)
    {
        var childRules = scopeRules
            .Where(rule => !string.IsNullOrWhiteSpace(rule.ChildName) &&
                           string.Equals(rule.SchemaName, schemaName, StringComparison.Ordinal) &&
                           string.Equals(rule.ObjectName, objectName, StringComparison.Ordinal))
            .ToList();
        if (childRules.Count == 0)
        {
            return children;
        }

        var blockedNames = childRules
            .Where(rule => IsBlockingKind(rule.ScopeKindCode))
            .Select(rule => rule.ChildName!)
            .ToHashSet(StringComparer.Ordinal);
        var includedNames = childRules
            .Where(rule => string.Equals(rule.ScopeKindCode, ScopeKindCodes.Include, StringComparison.Ordinal))
            .Select(rule => rule.ChildName!)
            .ToHashSet(StringComparer.Ordinal);

        var afterBlock = children
            .Where(child => !blockedNames.Contains(childNameSelector(child)))
            .ToList();

        // Bir Include child kurali varsa beyaz-liste kesin uygulanir: yalnizca adi listede olan child'lar kalir.
        // Bu taraf o child'i tasimasa bile karar degismez (simetri); tasimayan taraf bosalir, bu da dogru var/yok farkini uretir.
        return includedNames.Count > 0
            ? afterBlock.Where(child => includedNames.Contains(childNameSelector(child))).ToList()
            : afterBlock;
    }

    // islevi: Tek bir sema nesnesinin sema karsilastirmasina girip girmeyecegini belirler.
    // sistemdeki gorevi: Exclude/Ignore ancak nesne seviyeli (ChildName yok) ise nesnenin tamamini kapsar; Include ise ChildName tasisin tasimasin beyaz-listeyi etkinlestirir - "person.person.IsActive" include'u person.person nesnesini kapsama alir, kapsam disindaki nesneler (AuditLog vb.) beyaz-liste etkinken elenir. Nesne icindeki child daraltmasi FilterComparableChildren'da yapilir.
    public bool ShouldCompareObject(
        List<ComparisonScopeRule> scopeRules,
        string schemaName,
        string objectName)
    {
        // Nesnenin tamamini yalnizca ChildName tasimayan Exclude/Ignore bloklar; kolon seviyeli Exclude nesneyi degil child'i eler.
        var objectLevelRules = scopeRules
            .Where(rule => string.IsNullOrWhiteSpace(rule.ChildName))
            .ToList();
        if (HasBlockingRule(objectLevelRules, schemaName, objectName))
        {
            return false;
        }

        // Beyaz-liste: ChildName tasisin tasimasin her Include kurali etkinlestirir; nesne, kurallardan biriyle (sema + nesne) eslesmezse kapsam disidir.
        var includeRules = GetRules(scopeRules, ScopeKindCodes.Include).ToList();
        return includeRules.Count == 0 || includeRules.Any(rule => Matches(rule, schemaName, objectName));
    }

    // islevi: Tek bir tablonun satir sayisi/veri karsilastirmasina girip girmeyecegini belirler.
    public bool ShouldCompareDataForTable(
        List<ComparisonScopeRule> scopeRules,
        string schemaName,
        string tableName)
    {
        return ShouldCompareObject(scopeRules, schemaName, tableName) &&
               GetRules(scopeRules, ScopeKindCodes.DataCompare)
                   .Any(rule => MatchesExactObject(rule, schemaName, tableName));
    }

    // islevi: Exclude ve Ignore kurallarinin sema/nesne uzerinde bloklayici etkisini tek noktada toplar.
    private static bool HasBlockingRule(
        List<ComparisonScopeRule> scopeRules,
        string schemaName,
        string objectName)
    {
        return GetRules(scopeRules, ScopeKindCodes.Exclude)
                   .Concat(GetRules(scopeRules, ScopeKindCodes.Ignore))
                   .Any(rule => Matches(rule, schemaName, objectName));
    }

    // islevi: Istenen kural turundeki kurallari kararli ScopeKindCode'a gore secer.
    private static IEnumerable<ComparisonScopeRule> GetRules(
        List<ComparisonScopeRule> scopeRules,
        string scopeKindCode)
    {
        return scopeRules.Where(rule => string.Equals(rule.ScopeKindCode, scopeKindCode, StringComparison.Ordinal));
    }

    // islevi: Exclude ve Ignore turlerini tek noktada bloklayici (kara-liste) olarak isaretler.
    private static bool IsBlockingKind(string scopeKindCode)
        => string.Equals(scopeKindCode, ScopeKindCodes.Exclude, StringComparison.Ordinal) ||
           string.Equals(scopeKindCode, ScopeKindCodes.Ignore, StringComparison.Ordinal);

    // islevi: Sema seviyeli veya nokta-atisi nesne kuralinin verilen sema/nesneyle eslesip eslesmedigini belirler.
    private static bool Matches(
        ComparisonScopeRule rule,
        string schemaName,
        string objectName)
    {
        return string.Equals(rule.SchemaName, schemaName, StringComparison.Ordinal) &&
               (string.IsNullOrWhiteSpace(rule.ObjectName) ||
                string.Equals(rule.ObjectName, objectName, StringComparison.Ordinal));
    }

    // islevi: DataCompare gibi tablo-secimi isteyen kurallarda yalniz nokta-atisi sema.nesne eslesmesini kabul eder.
    private static bool MatchesExactObject(
        ComparisonScopeRule rule,
        string schemaName,
        string objectName)
    {
        return !string.IsNullOrWhiteSpace(rule.ObjectName) &&
               string.Equals(rule.SchemaName, schemaName, StringComparison.Ordinal) &&
               string.Equals(rule.ObjectName, objectName, StringComparison.Ordinal);
    }
}
