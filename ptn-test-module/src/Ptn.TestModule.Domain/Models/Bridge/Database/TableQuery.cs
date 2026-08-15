using System;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Schema checker'dan tek tablo bilgisi istemek icin baglanti ve konum referansini tasir.
// sistemdeki gorevi: Maliyetli DescribeTable cagrilarini serbest sorgu yerine tipli adresle sinirlar.
public sealed class TableQuery
{
    public Guid ConnectionId { get; set; }
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
}
