namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Fingerprint hesabina giren kanonik sema snapshot'ini tasir.
// sistemdeki gorevi: Sema durumunu provider DTO'sundan bagimsiz public kontrata verir.
public sealed class SchemaSnapshotDto
{
    /// <summary>
    /// Isleme katilan tablo adreslerini listeler.
    /// </summary>
    public List<SchemaTableDto> Tables { get; set; } = [];
}
