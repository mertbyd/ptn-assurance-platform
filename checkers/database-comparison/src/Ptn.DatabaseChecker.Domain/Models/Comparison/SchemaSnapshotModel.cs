using System;
using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Bir veritabaninin tek bir andaki tam sema fotografi (02 no'lu belgedeki kanonik model).
// sistemdeki gorevi: Her motor okuyucusu bu modeli doldurur; karsilastirma (diff) motoru yalnizca bu modeli gorur, verinin hangi motordan geldigini bilmez.
public class SchemaSnapshotModel : ExtensibleObject
{
    // Fotografin cekildigi motorun kararli kodu (DatabaseEngineCodes.*); ayni-motor mu capraz mi kararini ve guven seviyesini diff motoru buradan uretir, rapor basliginda lookup Name'i ile zenginlestirilir.
    public string EngineCode { get; set; } = string.Empty;

    // Baglanilan veritabaninin adi; rapor basliklarinda "Kaynak/Hedef" kimligi olarak gosterilir.
    public string DatabaseName { get; set; } = string.Empty;

    // Fotografin cekildigi an (UTC); iki taraf farkli zamanlarda okunduysa rapor bunu uyari olarak yazar.
    public DateTime CollectedAt { get; set; }

    // Veritabaninin varsayilan collation adi; kolon collation gurultusunu normalize etmek ve DB-seviyesi farki bulmak icin tasinir.
    public string? DatabaseCollationName { get; set; }

    // Veritabaninin collation provider kodu; PostgreSQL katalogu bildirirse dolar, SQL Server'da desteklenmedigi icin null kalir.
    public string? CollationProviderCode { get; set; }

    // Okunan tum tablolar; kolon/index/trigger alt nesneleriyle birlikte gelir.
    public List<SchemaTableModel> Tables { get; set; } = new();

    // Tablo olmayan sema nesneleri; view/function/procedure/sequence/type/extension tanimlari burada diff edilir.
    public List<SchemaObjectDefinitionModel> Objects { get; set; } = new();
}
