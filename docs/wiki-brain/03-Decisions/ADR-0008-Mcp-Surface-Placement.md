---
id: ADR-0008
type: decision
status: accepted
title: MCP surface lives in the composition host, not in checker packages
created: 2026-08-12
updated: 2026-08-12
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0002
  - ADR-0005
  - ADR-0007
rule_refs:
  - RULE-0001
  - RULE-0004
---

# ADR-0008 — MCP yüzeyi composition host'tadır, checker paketinde değil

## Bağlam

Araştırma sürecinde ayrı bir `CheckNexus.Assurance.Mcp` paketi önerilmişti: checker'ların
`Application.Contracts` katmanlarına bağlanan, tool'ları yayınlayan bir adapter. Gerekçe,
contracts-only bağımlılığın "MCP repository'ye erişemez" kuralını derleyiciyle zorlamasıydı.

Sonradan iki gerçek bu öneriyi geçersiz kıldı:

1. **MCP artık ayrı process gerektirmiyor.** `ModelContextProtocol.AspNetCore` mevcut ASP.NET Core
   uygulamasına `MapMcp()` ile eklenir; tool'lar controller'larla aynı Application servislerini
   çağırır, böylece doğrulama ve yetkilendirme ne tekrarlanır ne baypas edilir.
   MCP `2026-07-28` revizyonu stateless çekirdeğe geçtiği için sticky session veya paylaşılan
   session deposu da gerekmez.
2. **Tool kataloğu capability başına değil, ürün başına küratörlenir.** Tester'ın toplam tool
   bütçesi ~12'dir. Her checker kendi tool setini yayınlarsa katalog 20'yi aşar ve tool seçim
   doğruluğu ölçülebilir biçimde düşer. Hangi tool'un açılacağı, hangi izinle ve hangi tenant
   politikasıyla açılacağı yalnız composition host'un bilebileceği bir karardır.

## Karar

MCP sunucu yüzeyi **Test Module composition host'unda** yaşar. Checker paketleri MCP'ye dair
hiçbir tip, bağımlılık veya endpoint taşımaz.

Checker paketlerinin MCP'ye borcu, protokolden bağımsız üç şeydir ve hepsi zaten karşılanır:

- **Kararlı kod kümeleri** — `AssertionOutcomeCodes`, `HypothesisKindCodes`,
  `DiagnosisConfidenceCodes`, `DifferenceSeverityCodes`, `DifferenceKindCodes`.
- **Sınırlı çıktı** — assertion sonucu ~200 bayt, teşhis raporu ≤ 4 KB, bulgu sayfası ≤ 32 KB.
- **Sayfalama ve filtreleme** — bulgular tek parça dönmez.

## Alternatifler

- **Ayrı `CheckNexus.Assurance.Mcp` paketi:** capability başına katalog parçalanması ve
  ikinci bir sürümlenen yüzey maliyeti getirir; contracts-only kısıtı ADR ve kod incelemesiyle de korunabilir.
- **MCP tool'larını `HttpApi` projesine koymak:** paketi tüketen her consumer'a MCP bağımlılığı dayatır.
- **OpenAPI'den otomatik MCP tool üretimi:** endpoint sayısı kadar tool üretir; tool bütçesini tek başına tüketir.

## Sonuçlar ve riskler

Checker paket grafiği sade kalır; MCP sürüm değişimleri checker sürümlerini etkilemez.

Bu karar, checker'ların doğrudan HTTP ile de tüketilebilir olmasını zorunlu kılar — MCP bir
kolaylık katmanıdır, bağımlılık değildir. Endpoint sözleşmesi ve kararlı kodlar bu yüzden
`PACKAGE-README` içinde belgelenir.

İleride checker'ların dış kullanıcılar tarafından ajanla tüketilmesi istenirse, MCP Registry'ye
yayın ayrı bir ürün kararıdır ve bu ADR'yi supersede etmesi gerekir.
