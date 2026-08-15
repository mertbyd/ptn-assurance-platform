using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Database checker FK komsuluk DTO'sunun kaynak alan seklini tasir.
// sistemdeki gorevi: SchemaName anlam donusumunu Manager'a birakip Mapperly eslemesini attribute'suz tutar.
public sealed class CheckerForeignKeyNeighbor
{
    public string DirectionCode { get; set; } = string.Empty;
    public string ConstraintName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> LocalColumns { get; set; } = [];
    public List<string> NeighborColumns { get; set; } = [];
}
