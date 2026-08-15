using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Tek tablonun gelen veya giden bir seviyeli FK komsusunu tasir.
// sistemdeki gorevi: Binding yazarligina yonu ve kolon eslemesini provider-bagimsiz verir.
public sealed class ForeignKeyNeighbor
{
    public string DirectionCode { get; set; } = string.Empty;
    public string ConstraintName { get; set; } = string.Empty;
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> LocalColumns { get; set; } = [];
    public List<string> NeighborColumns { get; set; } = [];
}
