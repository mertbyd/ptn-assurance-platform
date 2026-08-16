# Test edilecek yazılımın iş kuralları

Bu dosya **test edilecek ürünün** iş kurallarını taşır. Yazarlık ajanının nasıl davranacağını
değil, doğrulanacak sistemin neyi garanti ettiğini anlatır. Ajan politikası ayrı kaynaktadır:
`agent-policy.md` (`ptn://authoring/agent-policy.md`). İki kaynak asla tek Resource'ta birleşmez.

Bu dosyanın içeriği **yorumlanmaz, adreslenir**. İçeriğin SHA-256 mührü `rules_fingerprint`
üretir ve yayın anında `test_scenarios.rules_fingerprint` kolonuna mühürlenir; dosya
değiştiğinde mühür tutmaz ve senaryo malzeme kaymasi olarak işaretlenir.

Aşağıdaki kurallar `Authoring/profiles/acme-ticketing.yaml` profiliyle aynı örnek alanı anlatır.
Gerçek kurulumda bu dosya ürünün kendi kurallarıyla değiştirilir; ayar
`TestModule.Bridge.BusinessRulesPath` kök yolu gösterir.

## Bilet yaşam döngüsü

- Bir bilet yalnız `Open -> InProgress -> Resolved -> Closed` sırasını izler; geriye dönüş yoktur.
- `Closed` bilet güncellenemez; yeniden açma yeni bilet kaydı üretir.
- Bir biletin atanabilmesi için `InProgress` durumunda olması zorunludur.

## Stok ve rezervasyon

- Bir rezervasyon oluştuğunda ilgili kalemin serbest stoğu tam olarak bir azalır.
- Rezervasyon iptal edildiğinde serbest stok tam olarak bir artar.
- Serbest stok hiçbir koşulda negatif olamaz.
- Toplam stok = serbest stok + rezerve stok; bu eşitlik her işlem sonrasında korunur.

## Kimlik ve tekillik

- Bir müşterinin aynı etkinlik için birden çok aktif rezervasyonu olamaz.
- Bilet numarası kurum genelinde tekildir ve yeniden kullanılmaz.

## Para ve iade

- İade tutarı ödenen tutarı aşamaz.
- Kısmi iade sonrası kalan iade hakkı = ödenen tutar - iade edilen toplam tutar.
