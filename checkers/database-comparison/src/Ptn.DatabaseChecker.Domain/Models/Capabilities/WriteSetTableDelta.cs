namespace Ptn.DatabaseChecker.Models.Capabilities;

// islevi: Tek aday tablonun capture oncesi/sonrasi kesin satir sayisini ve signed farkini tasir.
// sistemdeki gorevi: Logical veya diff gozlemini ham satir payload'i olmadan kucuk advisory ozete indirger.
public sealed class WriteSetTableDelta
{
    public string Table { get; set; } = string.Empty;
    public long BeforeRowCount { get; set; }
    public long AfterRowCount { get; set; }
    public long Delta { get; set; }
}
