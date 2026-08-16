# Ptn Assurance Platform

İş senaryosu testi platformunun kaynak kodu.

## İçerik

| Klasör | Ne |
|---|---|
| `ptn-test-module/` | Test Module — ABP modül katmanları ve composition host |

Bu depo yalnızca çalışan kaynak kodu track eder. Aşağıdakiler burada **track edilmez**:

| Ne | Nerede |
|---|---|
| `api-contract`, `database-comparison` checker'ları | Ayrı Git depoları |
| `CheckNexus.Vault` — ortak HashiCorp Vault adapteri | NuGet paketi olarak tüketilir |
| Mimari wiki, ADR'ler ve task metinleri | Bu deponun GitHub Wiki sekmesi |

## Derleme

```bash
dotnet build ptn-test-module/Ptn.TestModule.slnx
dotnet test  ptn-test-module/Ptn.TestModule.slnx
```

## Branch ve commit

- Her iş kendi branch'inde: `KBP-<no>`
- Bir branch = bir commit
- Commit biçimi: `#KBP-<no> <type>: <past-tense description>` — yeni iş için `feat: created ...`
