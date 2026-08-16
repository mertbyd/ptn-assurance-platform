namespace Ptn.DatabaseChecker.Dtos.Scopes;

// islevi: Tek bir kapsam kuralinin API modelidir (hem tarif girisi hem run snapshot cikisi).
// sistemdeki gorevi: Eski ScopeProfileItemDto'nun yerini alir; ScopeKind lookup'ina FK yerine kararli Code ile baglanir (owned JSON). FE, ScopeKindCode'u uygulama basinda yuklu ScopeKind lookup listesinden isme cozer.
public class ScopeRuleDto
{
    /// <summary>
    /// Kural turunun kararli teknik kodu (ScopeKindCodes.*): Include / Exclude / Ignore / DataCompare.
    /// </summary>
    public string ScopeKindCode { get; set; } = default!;

    /// <summary>
    /// Kuralin gecerli oldugu sema adi.
    /// </summary>
    public string SchemaName { get; set; } = default!;

    /// <summary>
    /// Kuralin hedef nesnesi; null ise tum sema anlamina gelir.
    /// </summary>
    public string? ObjectName { get; set; }

    /// <summary>
    /// Kuralin hedef alt nesnesi (kolon/index/constraint/trigger adi); null ise nesnenin tamami, dolu ise tablo icinde nokta atisi kolon/kisit filtresi.
    /// </summary>
    public string? ChildName { get; set; }
}
