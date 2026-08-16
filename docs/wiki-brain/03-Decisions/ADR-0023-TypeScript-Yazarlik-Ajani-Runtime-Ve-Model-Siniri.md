---
id: ADR-0023
type: decision
status: accepted
title: TypeScript yazarlik ajani runtime, MCP ve model adapter siniri
created: 2026-08-16
updated: 2026-08-16
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0008
  - ADR-0014
  - ADR-0017
  - ADR-0018
rule_refs:
  - RULE-0005
  - RULE-0006
---

# ADR-0023 — TypeScript yazarlık ajanı runtime ve model sınırı

## Bağlam

KBP-111 doğal dil girdisini Test Module'ün deterministik yazarlık yüzeyine bağlayan ayrı bir
ajan gerektirir. Ajan sohbet ve streaming sunacak, MCP Resource ve tool'larını kullanacak,
fakat checker paketlerine veya herhangi bir ürün veritabanına doğrudan erişmeyecektir.
RULE-0005 gereği model yalnız yazarlık anında bulunabilir; koşum ve yargı grafiğine giremez.

Yeni proje grafiği kodlanmadan önce runtime, paketleme, transport, model ve secret sınırlarının
tek kararda sabitlenmesi gerekir. 2026-08-16 tarihinde yerel doğrulama Node `v24.19.0` ve pnpm
`11.19.0` verdi. Resmî Node sürüm tablosu v24'ü LTS, v26'yı Current gösterir; üretim yalnız
LTS hattını kullanır.

## Karar

### A. Kök, runtime ve paket yöneticisi

Yeni uygulama repository kökündeki `ptn-test-agent/` altında yaşar. Runtime **Node.js 24 LTS**,
modül biçimi ESM ve dil TypeScript'tir. `package.json` `engines.node` ile `>=24 <25` sınırını,
`packageManager` ile **pnpm 11.19.0** sürümünü sabitler. Lockfile commit edilir; CI ve faz kapısı
`pnpm install --frozen-lockfile` kullanır.

Node 26 Current geliştirme hedefi değildir. LTS ana sürümü değişimi yeni ölçüm ve bu ADR'nin
yerine geçen bir karar gerektirir.

### B. HTTP ve streaming

Tarayıcı/UI yüzeyi **Fastify 5** üzerinde çalışır. Chat/session, mesaj, upload ve iptal uçları
JSON HTTP; model metin akışı standart **Server-Sent Events** olarak sunulur. Fastify yalnız
transport, body bütçesi, abort sinyali ve redacted logging sahibidir; agent döngüsü framework
handler'ına gömülmez.

Dosya yükleme multipart genel amacıyla açılmaz. Yalnız UTF-8 metin olarak `senaryo.md` ve
`kurallar.md` adları, ayrı içerik bütçesi ve runtime şemasıyla kabul edilir.

### C. MCP istemcisi

Ajanın Test Module'e tek erişim yolu `/mcp` üzerindeki Streamable HTTP transport'tur.
`@modelcontextprotocol/client` **2.0.0** kullanılır. İstemci v2'nin geriye uyumlu varsayılan
legacy handshake'i ile mevcut .NET sunucuya bağlanır; protokol yükseltmesi otomatik ürün kararı
değildir.

Bearer token yalnız `PTN_MCP_BEARER_TOKEN` environment secret'ından auth provider'a verilir.
Bağlantı sonrası sunucu instructions, Resource'lar, discoverable tool listesi ve an profili
okunur. Model tool listesi bu canlı profilden kurulur; sabit veya genişletilmiş yerel tool
kataloğu tutulmaz. Tool input ve output'ları Zod runtime şemalarından geçmeden agent durumuna
alınmaz.

### D. Model adapter sınırı

Domain ve agent döngüsü sağlayıcı SDK tipi bilmez. `ModelAdapter` portu streaming metin,
yapılandırılmış function call, token kullanımı ve iptal sinyalinden oluşur. İlk adapter
**OpenAI Responses API** ve resmî `openai` **7.4.0** JavaScript SDK'sıdır. Responses function
calling'in JSON Schema tool tanımları ve `call_id` ile tool output geri besleme döngüsü
kullanılır; final Arazzo YAML hiçbir model çağrısının çıktı şeması değildir.

Model kimliği `AGENT_MODEL` environment değişkeninde zorunludur; kodda `latest` veya sağlayıcıya
özel varsayılan model sabitlenmez. Yeni sağlayıcı aynı portu uygulayabilir. Yerel model için
ürün desteği ancak KBP-111 değerlendirmesinde tool-selection F1 `>= 0.90` kanıtlanırsa açılır.

> [!WARNING] Gerçeklik sapması — kurulan ajan bu maddeyi uygulamıyor (2026-08-16)
> `ptn-test-agent` içinde **`OpenAIModelAdapter` yoktur**. Kurulan adapter'lar
> `OllamaModelAdapter` (yerel **qwen3:8b**) ve test için `FakeModelAdapter`'dır; `openai` SDK'sı
> bağımlılık listesinde değildir. Provider-neutral `ModelAdapter` portu, Zod fail-closed
> doğrulaması, tek onarım turu ve tool bütçesi **tuttu** — bu maddenin asıl koruması ayakta.
> Sapan tek şey ilk adapter'ın kimliğidir; §F'deki `OPENAI_API_KEY` şartı da bu kurulumda
> uygulanmaz. Yerel model F1 `>= 0.90` kapısı ölçülmedi.
>
> Bu not sapmayı **kaydeder, kararı değiştirmez** (wiki kuralı: karar sessizce yeniden yazılmaz).
> Yerel-öncelikli model sınırını kalıcı hâle getirmek ADR-0023'ü kısmen `supersedes` eden yeni
> bir ADR ister ve **ürün sahibinin kararıdır**.

### E. Bütçe, belirsizlik ve onay

İstemci `maxTurns` ve toplam token bütçesini her model çağrısından önce uygular; MCP sunucusunun
kararı daha dardır ve daima üstündür. `input_required` geldiğinde döngü durur ve kapalı seçenekler
UI'a taşınır; model cevap uyduramaz. Model yalnız tek `AddAuthoringStep` biçimi önerebilir.
Kademe-4 eylem otomatik uygulanmaz ve discoverable tool olarak genişletilmez.

### F. Secret ve gözlemlenebilirlik

`OPENAI_API_KEY` ve `PTN_MCP_BEARER_TOKEN` yalnız process environment veya deployment secret
store'dan gelir. Config şeması başlangıçta eksik secret'ı reddeder. Secret değerleri DTO,
SSE event, log, trace attribute, exception metni veya test fixture'ına yazılmaz. Fastify log
redaction listesi authorization, cookie ve API-key header/path'lerini kapsar. Ham model
input/output kalıcılaştırılmaz; yalnız conversation kimliği, model referansı ve token sayaçları
OpenTelemetry GenAI sözlüğüyle ölçülür.

### G. Deployment sahibi

`ptn-test-agent` ayrı process/container ve ayrı deployable'dır; `.NET` Test Module host'una
gömülmez. Deployment sahibi **Test Platform ekibi**dir. UI yalnız agent HTTP/SSE yüzeyini,
agent yalnız yetkili Test Module MCP ve seçili model provider'ı görür. Test Module, checker ve
runner projeleri agent paketine dependency alamaz.

## Alternatifler

- **Model istemcisini .NET host'a eklemek.** Koşum/yargı grafiğine sızma riskini ve provider
  paketini deterministik backend'e taşır. RULE-0005 ve görev yasağı nedeniyle reddedildi.
- **OpenAI Agents SDK ile provider sınırını birleştirmek.** İlk sürümün ihtiyacı küçük ve MCP
  kararları ürün-domain kurallarıdır; adapter ile doğrudan Responses API daha dar ve test
  edilebilirdir. İleride ölçülmüş gereksinim olursa ayrı ADR ister.
- **Express veya özel `node:http` sunucusu.** Express daha geniş middleware yüzeyi getirir;
  çıplak HTTP validation, body budget ve redaction altyapısını yeniden yazdırır. Fastify'ın
  şema/streaming sınırı daha küçüktür.
- **Node 26 Current.** Üretim için LTS değildir. Reddedildi.
- **npm/yarn veya lockfilesiz kurulum.** Yerel araç zinciri pnpm sunar; frozen lockfile faz
  kapısının tekrarlanabilirliğini sağlar.
- **Sabit yerel tool kataloğu.** Sunucunun an profili ve tool budget kararını baypas eder.
  Reddedildi.

## Sonuçlar ve riskler

| Risk | Önlem |
|---|---|
| Provider API değişimi | SDK yalnız `OpenAIModelAdapter` içinde; port ve fixture testleri provider bağımsız |
| MCP protokol drift'i | v2 istemci, geriye uyumlu handshake ve integration test; sürüm yükseltmesi lockfile diff'iyle görünür |
| SSE kopması | AbortController model ve MCP çağrılarına taşınır; session terminal durumu korunur |
| Tool şeması drift'i | Canlı discovery + Zod input/output validation; bilinmeyen çıktı fail-closed |
| Secret sızıntısı | Startup config validation, logger redaction ve repository secret scan |
| Yerel model iddiasının ölçümsüz açılması | F1 `>= 0.90` kabul kapısı; altında adapter deneysel bile sunulmaz |

## Kaynaklar

- https://nodejs.org/en/about/previous-releases
- https://fastify.dev/docs/latest/Reference/LTS/
- https://github.com/modelcontextprotocol/typescript-sdk/blob/main/docs/client.md
- https://github.com/modelcontextprotocol/typescript-sdk/blob/main/docs/get-started/packages.md
- https://developers.openai.com/api/docs/guides/function-calling
- https://developers.openai.com/api/docs/guides/streaming-responses
