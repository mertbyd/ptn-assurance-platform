namespace Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

// islevi: SQL Server INFORMATION_SCHEMA.SEQUENCES satirini EF Core okumasi icin temsil eder.
// sistemdeki gorevi: Sequence detaylarini sql_variant alanlara girmeden schema snapshot'in tablo disi nesne listesine tasir.
public sealed class SqlServerSequenceCatalogRow
{
    // INFORMATION_SCHEMA.SEQUENCES.SEQUENCE_SCHEMA: sequence semasi.
    public string Schema { get; set; } = string.Empty;

    // INFORMATION_SCHEMA.SEQUENCES.SEQUENCE_NAME: sequence adi.
    public string Name { get; set; } = string.Empty;

    // INFORMATION_SCHEMA.SEQUENCES.DATA_TYPE: sequence sayisal veri tipi.
    public string DataType { get; set; } = string.Empty;

    // INFORMATION_SCHEMA.SEQUENCES.START_VALUE: baslangic degeri.
    public string StartValue { get; set; } = string.Empty;

    // INFORMATION_SCHEMA.SEQUENCES.MINIMUM_VALUE: minimum deger.
    public string MinimumValue { get; set; } = string.Empty;

    // INFORMATION_SCHEMA.SEQUENCES.MAXIMUM_VALUE: maksimum deger.
    public string MaximumValue { get; set; } = string.Empty;

    // INFORMATION_SCHEMA.SEQUENCES.INCREMENT: artis miktari.
    public string Increment { get; set; } = string.Empty;

    // INFORMATION_SCHEMA.SEQUENCES.CYCLE_OPTION: cycle davranisi.
    public string CycleOption { get; set; } = string.Empty;
}
