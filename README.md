# Ptn Assurance Platform

İş senaryosu testi platformunun kaynak kodu.

## İçerik

| Klasör | Ne |
|---|---|
| `ptn-test-module/` | Test Module — ABP modül katmanları ve composition host |
| `vault/` | `CheckNexus.Vault` — iki checker'ın ortak HashiCorp Vault adapteri |

İki checker modülü (`api-contract`, `database-comparison`) **ayrı Git depolarıdır** ve burada
track edilmez.

## Derleme

```bash
dotnet build ptn-test-module/Ptn.TestModule.slnx
dotnet test  ptn-test-module/Ptn.TestModule.slnx
```

## Branch ve commit

- Her iş kendi branch'inde: `KBP-<no>`
- Bir branch = bir commit
- Commit biçimi: `#KBP-<no> <type>: <past-tense description>` — yeni iş için `feat: created ...`
