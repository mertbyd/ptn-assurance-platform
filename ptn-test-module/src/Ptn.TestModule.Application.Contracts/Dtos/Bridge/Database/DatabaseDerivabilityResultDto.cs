namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: DB assertion turetilebilirlik listesini ve birlesik kapinin sonucunu tasir.
// sistemdeki gorevi: Tek turetilemeyen assertion'in yayin adayini sessizce gecmesini engeller.
public sealed class DatabaseDerivabilityResultDto
{
    /// <summary>
    /// Ilgili degerleri kararli sirada listeler.
    /// </summary>
    public List<DatabaseDerivabilityItemDto> Assertions { get; set; } = [];
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool AllDerivable { get; set; }
}
