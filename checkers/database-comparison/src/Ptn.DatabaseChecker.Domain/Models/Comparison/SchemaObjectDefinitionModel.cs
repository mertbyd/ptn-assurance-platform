using Volo.Abp.ObjectExtending;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Tablo disi sema nesnelerinin (view, routine, sequence, type, extension) kanonik tanim kaydidir.
// sistemdeki gorevi: Snapshot okuma akisinda tablo alt detaylarina gomulemeyen DBMS nesnelerini tek ortak modelde tasir; diff motoru provider katalog satirlarini degil yalniz bu modeli gorur.
public class SchemaObjectDefinitionModel : ExtensibleObject
{
    // Nesnenin ait oldugu sema; extension gibi DB geneli nesnelerde extension'in kurulu oldugu namespace kullanilir.
    public string Schema { get; set; } = string.Empty;

    // Nesne adi; Schema + ObjectTypeCode ile birlikte diff anahtarinin insan okunur parcasidir.
    public string Name { get; set; } = string.Empty;

    // Nesne turunun kararli lookup kodu (SchemaObjectTypeCodes.*).
    public string ObjectTypeCode { get; set; } = string.Empty;

    // Motorun veya okuyucunun urettigi raporlanabilir tanim; whitespace/format farklari compare fazinda normalize edilir.
    public string Definition { get; set; } = string.Empty;
}
