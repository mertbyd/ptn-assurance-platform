namespace Ptn.DatabaseChecker.Models.Comparison.Scope;

// islevi: Tek bir kapsam kurali (bak / bakma / yok say / veri say); entity DEGIL, ComparisonDefinition'a (yeniden kullanilabilir tarif) ve ComparisonRun snapshot'ina owned JSON olarak gomulur.
// sistemdeki gorevi: Motor calisirken Include/Exclude/Ignore/DataCompare kurallarini bu modelden okur; eski ScopeProfileItem/ComparisonRunScopeItem tablolarinin yerini alir. Lookup'a FK yerine kararli Code ile baglanir (donmus snapshot: JSON icinde FK tutmak tarih kaydini kirardi).
public class ComparisonScopeRule
{
    // Kural anlami lookup'inin kararli kodu (ScopeKindCodes.*): Include / Exclude / Ignore / DataCompare. FK degil, snapshot icinde donmus kod; gecerliligi validator, kararli kod setine karsi dogrular.
    public string ScopeKindCode { get; set; } = default!;

    // Kuralin gecerli oldugu sema (public, dbo, muhasebe...).
    public string SchemaName { get; set; } = default!;

    // Kuralin hedef nesnesi; null = semanin tamami (geleceye acik), dolu = nokta atisi.
    public string? ObjectName { get; set; }

    // Kuralin hedef alt nesnesi (kolon/index/constraint/trigger adi); null = nesnenin tamami, dolu = tablo icinde nokta atisi kolon/kisit filtresi.
    public string? ChildName { get; set; }
}
