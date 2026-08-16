using System;
using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Bir veritabaninin tek andaki tam sema fotografinin API cevap modelidir.
// sistemdeki gorevi: Derin okuyucunun urettigi SchemaSnapshotModel'i frontend'e tasir; karsilastirma oncesi bir DB'nin tablo/kolon yapisini komple gosterir.
public class SchemaSnapshotDto
{
    /// <summary>
    /// Fotografin cekildigi motorun kararli kodu (DatabaseEngineCodes.*).
    /// </summary>
    public string EngineCode { get; set; } = default!;

    /// <summary>
    /// Baglanilan veritabaninin adi.
    /// </summary>
    public string DatabaseName { get; set; } = default!;

    /// <summary>
    /// Fotografin cekildigi an (UTC).
    /// </summary>
    public DateTime CollectedAt { get; set; }

    /// <summary>
    /// Veritabaninin varsayilan collation adi.
    /// </summary>
    public string? DatabaseCollationName { get; set; }

    /// <summary>
    /// Provider destekliyorsa veritabaninin collation provider kodu.
    /// </summary>
    public string? CollationProviderCode { get; set; }

    /// <summary>
    /// Okunan tablolar; her biri kolonlariyla birlikte gelir.
    /// </summary>
    public List<SchemaTableDto> Tables { get; set; } = new();

    /// <summary>
    /// Tablo olmayan sema nesneleri; view/function/procedure/sequence/type/extension tanimlari.
    /// </summary>
    public List<SchemaObjectDefinitionDto> Objects { get; set; } = new();
}
