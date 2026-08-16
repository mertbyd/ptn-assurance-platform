using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: DescribeTable cevabinda hedef tabloyla dogrudan FK bagi bulunan tek komsuyu tasir.
// sistemdeki gorevi: Bir seviye iliski baglami sunar; otomatik binding veya serbest sorgu uretmez.
public class ForeignKeyNeighborDto
{
    public string DirectionCode { get; set; } = string.Empty;
    public string ConstraintName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> LocalColumns { get; set; } = new();
    public List<string> NeighborColumns { get; set; } = new();
}
