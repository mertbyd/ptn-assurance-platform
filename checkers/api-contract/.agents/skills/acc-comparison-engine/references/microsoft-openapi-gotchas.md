# Microsoft.OpenApi ile çalışırken

Sürüm pini ve lisans: [kanonik API Contract Checker gerçeği](../../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#7-api-contract-checker-gercegi).
Burası **API davranışı** hakkında; sürüm bilgisi orada tutulur, burada tekrar edilmez.

## Okuma

```csharp
var settings = new OpenApiReaderSettings
{
    RuleSet = ValidationRuleSet.GetEmptyRuleSet()   // ucuncu tarafi denetlemiyoruz
};
settings.AddYamlReader();   // YAML ayri pakettedir; cagrilmazsa YAML spec sessizce reddedilir
var (document, diagnostics) = await OpenApiDocument.LoadAsync(stream, settings: settings);
```

- Biçim verilmezse önce JSON, sonra (kayıtlıysa) YAML denenir.
- `diagnostics` **her zaman** kontrol edilir — ama ne ölçtüğü `RuleSet`'e bağlıdır.
  Varsayılan ruleset stil ihlallerini de `Errors`'a yazar ve bunlar gerçek okuma
  hatalarından **tip olarak ayrılamaz** (ikisi de `OpenApiValidatorError`). Boş
  ruleset ile `Errors` yalnız okuyucu düzeyinde kalır: kısmi çözülme yakalanır,
  üçüncü tarafın stil ihlali ingestion'ı kırmaz.
- Tanınmayan gövde (HTML hata sayfası gibi) `Errors` üretmez,
  `OpenApiUnsupportedSpecVersionException` **fırlatır** — yakalanmazsa 500 olur.

## v1'den taşınan varsayımlar — hepsi yanlış

| Eski varsayım | Gerçek |
|---|---|
| `schema.Nullable == true` | `schema.Type` bir **bayrak kümesi**: `JsonSchemaType.String \| JsonSchemaType.Null` |
| `operation.OperationType` enum | `HttpMethod` **nesnesi** |
| Koleksiyonlar boş liste olarak gelir | **null gelebilir** — model kurulurken önceden ayrılmaz |
| Çözülemeyen `$ref` exception atar | **uyarı** üretir; `diagnostics`'e bakmazsan fark etmezsin |
| Sayısal sınırlar `decimal` | **string** tutulur (hassasiyet kaybını önlemek için) |

Bu beş madde motorun her satırını etkiler. Özellikle **null koleksiyon**: her
`foreach` öncesi null kontrolü ya da `?? []` gerekir.

## Referans çözümü

- Referanslar **tembel proxy** ile çözülür (`Target` / `RecursiveTarget`).
  Erişmeden çözülmez; bu yüzden derin karşılaştırmada döngüsel referans riski
  vardır — ziyaret edilen şema kimliklerini bir `HashSet` ile takip et.
- Çok dosyalı spec: `document.Workspace.RegisterComponents(otherDocument)`.
- Fark adresinde **referans kimliğini koru**. `$ref` çözüldü diye bulguyu
  "inline şema" olarak raporlamak, kullanıcının hangi DTO'nun değiştiğini
  görmesini engeller.

## Nullable karşılaştırması — en sık hata

3.0 ve 3.1 aynı gerçeği farklı yazar. Karşılaştırmadan **önce** tek forma indir:

```csharp
// 3.0:  { "type": "string", "nullable": true }
// 3.1:  { "type": ["string", "null"] }
// ikisi de ayni sey -> kanonik modelde tek alan
```

İndirgemeden karşılaştırırsan aynı API'nin 3.0 ve 3.1 sürümleri arasında **her
alan** için sahte fark üretirsin.

## Sürüm çakışması riski

`Volo.Abp.Swashbuckle` dolaylı olarak bir `Microsoft.OpenApi` sürümü çeker.
Doğrudan referansımızla çakışabilir. Bu, KBP-603'ün **ilk** doğrulama maddesidir;
sonucu CURRENT-0002'ye işlenir.

Belirti: derleme geçer, çalışma zamanında `MissingMethodException` veya
`TypeLoadException`. Çözüm sırası: (1) sürümleri hizala, (2) hizalanamıyorsa
`Microsoft.AspNetCore.OpenApi`/Swashbuckle tarafını sabitle, (3) hâlâ olmuyorsa
okuma işini ayrı bir sürece taşımayı **ADR ile** öner.
