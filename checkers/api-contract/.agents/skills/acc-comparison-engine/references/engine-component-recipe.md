# Format bileşeni ekleme tarifi

Yeni bir spec formatı (veya formata özgü davranış) eklemek **kapalı bir iştir**:
yeni bileşen + yeni format kodu. Manager, AppService ve controller **değişmez**.
Değişiyorsa soyutlama sızmıştır — dur ve soyutlamayı düzelt.

## Mekanizma

```text
ISpecFormatComponent  <- her formata ozgu arayuz bunu turer
    ^
ISpecReader, ISpecFetcher, ...
    ^
OpenApi31SpecReader, OpenApi30SpecReader, Swagger20SpecReader
    ^
SpecFormatComponentResolver<ISpecReader>.Resolve(formatCode)
```

`SpecFormatComponentResolver<T>` tek açık-generic kayıtla bağlanır (EFCore modülünde);
bileşenlerin kendisi `ITransientDependency` ile otomatik toplanır.

## Adımlar

1. **Format kodunu ekle.** `Domain.Shared/Constants/{Alan}/Lookups/SpecFormatCodes.cs`
   içinde kararlı bir sabit ([kanonik checker kuralları](../../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#13-checker-gelistirme-kurallari)).
2. **Lookup satırını seed et.**
   [`../../acc-lookup-recipe/SKILL.md`](../../acc-lookup-recipe/SKILL.md).
3. **Arayüzü tanımla** (yalnız ilk formatta): `Domain/Interface/Comparison/ISpecReader.cs`,
   `ISpecFormatComponent` türer, `FormatCode` özelliğini taşır.
4. **Bileşeni yaz:** `EntityFrameworkCore/Comparison/{Format}SpecReader.cs`.
5. **Testi yaz:** resolver'ın o kodu bu sınıfa çözdüğünü kanıtla.

## Adlandırma — en pahalı tuzak

> **Sınıf adı, arayüz adının `I`'siz hali ile BİTMEK ZORUNDADIR.**

```text
ISpecReader          -> OpenApi31SpecReader      ✅
ISpecReader          -> OpenApi31Reader          ❌ DI hic gormez
ISpecReader          -> SpecReaderOpenApi31      ❌ DI hic gormez
IDatabaseConnectionTester -> PostgreSqlDatabaseConnectionTester ✅ (dbchecker ornegi)
```

Yanlış adda **derleme geçer**, bileşen DI'da hiç görünmez, resolver
"desteklenmeyen format" fırlatır. dbchecker'da 2026-07-08'de canlı hata olarak
yaşandı. Sessiz başarısızlık olduğu için testle sabitlenir.

## İskelet

```csharp
namespace Ptn.ApiContractChecker.Comparison;

// islevi: OpenAPI 3.1 dokumanini kanonik modele cevirir.
// sistemdeki gorevi: Format-ozgu okuma bilgisini tek sinifta tutar; motor formatı bilmez.
public class OpenApi31SpecReader : ISpecReader, ITransientDependency
{
    // Resolver'in bu bileseni sectigi kararli format kodu.
    public string FormatCode => SpecFormatCodes.OpenApi31;

    // islevi: Ham spec akisini kanonik dokumana cevirir; ayristirma tanilari yutulmaz.
    public async Task<CanonicalApiDocument> ReadAsync(Stream content)
    {
        // ...
    }
}
```

## Doğru soyutlama testi

Yeni format eklerken aşağıdakilerden **herhangi biri** değiştiyse dur:

- bir manager
- bir AppService
- bir controller
- bir DTO
- bir `switch` / `if (formatCode == …)` bloğu

Bunlar soyutlamanın sızdığını gösterir. Formata özgü davranış **yalnız** bileşenin
içinde yaşar.
