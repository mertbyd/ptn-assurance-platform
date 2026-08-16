namespace Ptn.DatabaseChecker.Dtos.Capabilities;

// islevi: Tek tablonun capture oncesi/sonrasi satir sayilarini ve signed farkini public cevapta tasir.
// sistemdeki gorevi: Tuketiciye ham degisiklik kaydi yerine kucuk, provider-notr delta ozeti verir.
public sealed class WriteSetTableDeltaDto
{
    public string Table { get; set; } = string.Empty;
    public long BeforeRowCount { get; set; }
    public long AfterRowCount { get; set; }
    public long Delta { get; set; }
}
