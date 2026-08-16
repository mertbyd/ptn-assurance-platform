namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: DescribeTable cevabinda tek schema lint uyarisini kararli kod ve opsiyonel kolon adresiyle tasir.
// sistemdeki gorevi: Senaryo yazim/yayin kapisinin provider mesaji parse etmeden tablo risklerini yorumlamasini saglar.
public sealed class SchemaLintWarningDto
{
    public string WarningCode { get; set; } = string.Empty;
    public string? ColumnName { get; set; }
}
