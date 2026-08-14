using System;

namespace Ptn.TestModule.Models.Bridge;

// islevi: API checker'dan operasyon baglama veya istek ornegi istemek icin snapshot adresini tasir.
// sistemdeki gorevi: Domain portunu checker DTO'sundan ve serbest operasyon tahmininden ayirir.
public sealed class PtnOperationQuery
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}
