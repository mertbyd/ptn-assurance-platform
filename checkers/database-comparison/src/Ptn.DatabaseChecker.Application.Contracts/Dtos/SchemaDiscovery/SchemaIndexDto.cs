using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Tek bir index'in API cevap modelidir.
// sistemdeki gorevi: Schema snapshot cevabinda index kolonlari, include kolonlari ve filtre/tanim bilgisini tasir.
public class SchemaIndexDto
{
    /// <summary>
    /// Index adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Unique index mi.
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Primary key'i destekleyen index mi.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Index anahtar kolonlari.
    /// </summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>
    /// INCLUDE kolonlari.
    /// </summary>
    public List<string> IncludedColumns { get; set; } = new();

    /// <summary>
    /// Filtered/partial index WHERE ifadesi.
    /// </summary>
    public string? FilterDefinition { get; set; }

    /// <summary>
    /// Provider'in index icin urettigi ham tanim metni.
    /// </summary>
    public string? Definition { get; set; }
}
