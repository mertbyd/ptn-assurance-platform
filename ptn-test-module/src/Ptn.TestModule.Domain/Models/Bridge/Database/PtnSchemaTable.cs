using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Sema snapshot'indaki tek tablonun kanonik kolon kimliklerini tasir.
// sistemdeki gorevi: Sirali SHA-256 hesabinin minimum ve kararli girdisini olusturur.
public sealed class PtnSchemaTable
{
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
}
