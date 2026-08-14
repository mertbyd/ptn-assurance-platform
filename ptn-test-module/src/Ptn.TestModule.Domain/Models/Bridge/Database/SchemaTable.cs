using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Sema snapshot'indaki tek tablonun kanonik kolon kimliklerini tasir.
// sistemdeki gorevi: Sirali SHA-256 hesabinin minimum ve kararli girdisini olusturur.
public sealed class SchemaTable
{
    public string Schema { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<SchemaColumn> Columns { get; set; } = [];
}
