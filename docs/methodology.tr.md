# Yöntem

Bu sayfa raporun 2026 sürümünü anlatır. İngilizce metin (`docs/methodology.md`) esas alınır; bu
sayfa onun Türkçe çevirisidir.

## Kısaca

- **Neyi kontrol ettik:** Gerçek Chrome ziyaret verilerine göre Türkiye'de en çok ziyaret edilen
  1.000 web sitesinin ana sayfasını.
- **Nasıl:** Bir bilgisayar programı her ana sayfayı normal bir ziyaretçi gibi bir kez açtı ve
  üzerinde açık kaynaklı bir kontrol aracı (axe-core) çalıştırdı; hem bilgisayar hem telefon
  ekranı boyutunda.
- **Neye baktık:** Siteyi engelli kişiler için zorlaştıran sorunlara; örneğin metin açıklaması
  olmayan görseller, okunamayacak kadar soluk yazılar veya ekran okuyucunun okuyabileceği bir adı
  olmayan düğmeler.
- **Ne değildir:** Yasal bir denetim değildir. Bir program sorunların yalnızca bir kısmını
  bulabilir; "sorun bulunmadı" demek sitenin herkes için kolay olduğu anlamına gelmez.
- **Kurallara uyduk:** Her sitenin robots.txt kurallarına uyduk, her ana sayfayı yalnızca bir kez
  açtık ve bir site ziyaretimizi reddettiğinde durduk. Raporda siteleri sıralamıyor ve isim
  vermiyoruz.
- **Site sahipleri** rapordan çıkarılmayı isteyebilir (bkz. "Kapsam dışı kalma").

Sayfanın geri kalanı teknik ayrıntıları anlatır.

## Bu rapor nedir, ne değildir

Bu rapor, Türkiye'deki popüler web sitelerinin **ana sayfalarında**, açık kaynaklı axe-core
motoru ve WCAG 2.0, 2.1 ve 2.2 A ve AA kurallarıyla **otomatik olarak tespit edilebilen**
erişilebilirlik hatalarını ölçer.

Bu rapor bir yasal uygunluk denetimi veya sertifika **değildir**. Hiç hata tespit edilmeyen bir
sayfa, erişilebilir olduğu anlamına **gelmez**: otomatik araçlar, gerçek kullanıcıların karşılaştığı
engellerin yalnızca bir kısmını bulur. Klavyeyle kullanım, ekran okuyucu deneyimi, anlamlı
alternatif metinler ve daha birçok konu insan tarafından test edilmelidir.

Herkese açık rapor **sektör düzeyinde toplu sonuçlar** gösterir. Siteleri sıralamaz ve "en kötü"
site listeleri yayımlamaz.

## Hangi siteler

- **Kaynak liste:** Chrome UX Report (CrUX) Türkiye popülerlik verisi, Ekim 2026 kopyası
  (`tr-202610`). CrUX, bir ülkedeki gerçek Chrome sayfa yüklemelerine göre origin'leri sıralar ve
  bunları tam sıra yerine kovalar halinde gruplar (ilk 1.000, 5.000, 10.000, …).
- **Seçim:** `.tr` altındaki alan adları (ve şehir alan adları `.istanbul` ile `.ist`), ayrıca
  başka uzantılarda olup elle listelediğimiz tanınmış Türk siteleri; en fazla 1.000 alan adı.
  Rapor Türkiye'yi kapsadığı için `.nc.tr` (Kuzey Kıbrıs) dışarıda bırakıldı.
- **Site başına tek kayıt:** Bir alan adının tüm origin'leri (örneğin `www.`, `m.` veya bir öğrenci
  portalı alt alan adı) tek site sayılır ve aralarındaki en iyi kova kullanılır. Ana host'un
  (`alanadi` veya `www.alanadi`) ana sayfası taranır. CrUX'ta yalnızca alt alan adlarıyla görünen
  alan adları (örneğin bir e-devlet uygulaması veya giriş portalı) listeye alınmadı, çünkü
  popülerlikleri bir ana sayfadan gelmiyor.
- **Sıra ve kesme noktası:** Siteler kovaya göre sıralanır. CrUX bir kovanın içindeki siteleri
  sıralamadığı için, son kovadaki 1.000 sınırı sabit bir sözde rastgele sırayla (alan adının
  SHA-256 özeti) belirlenir; bu sıra kararlıdır ve alfabetik değildir.
- **Tekrar ayıklama:** Ana sayfası, aynı taramada zaten taranmış başka bir siteye yönlenen bir
  site bir kez sayılır ("tekrar" olarak işaretlenir).
- **Sektörler:** kamu (belediyeler dahil), bankacılık ve finans, e-ticaret, üniversiteler, haber ve
  medya, diğer. Alan adı kurallarıyla (`.gov.tr`, `.bel.tr`, `.pol.tr`, `.mil.tr`, `.tsk.tr` → kamu;
  `.edu.tr` → üniversite) ve diğer tüm alan adlarının elle incelenmesiyle atanır. Bilinmeyen
  siteler "diğer" sayılır.

## Her sayfa nasıl taranır

- Her taramada site başına bir sayfa yüklemesi yapılır; buna tarayıcının kendiliğinden yüklediği
  kaynaklar eklenir.
- Chromium (Playwright ile), masaüstü görünüm 1280×800, dil ayarı `tr-TR`.
- Mobil kontrol: Masaüstü kontrolünden sonra aynı yüklenmiş sayfa 390×844 boyutuna küçültülüp
  yeniden kontrol edilir; böylece ikinci bir sayfa yüklemesi gerekmez. Bu nedenle mobil
  tarayıcılara farklı sayfa gönderen siteler (örneğin ayrı `m.` siteleri) mobil kontrolün
  kapsamında değildir.
- HTML belgesinin hazır olması (`DOMContentLoaded`) için en fazla 30 saniye, ardından sayfanın load
  olayı için en fazla 10 saniye daha ve sonra 2 saniye beklenir. Birçok popüler sayfa, içeriği
  hazır olduktan çok sonra da reklam ve takip kodu yüklemeye devam eder; load olayı bu sürede
  gelmezse sayfa yine de kontrol edilir ve tam yüklenmeden kontrol edildiği kaydedilir.
- Etkileşim yoktur: tıklama, kaydırma, çerez bannerını kabul etme veya form gönderme yapılmaz.
- Chromium 156 (Playwright 1.64.1) içinde axe-core 4.14.0. Kural etiketleri: `wcag2a`, `wcag2aa`,
  `wcag21a`, `wcag21aa`, `wcag22aa`.
- Sayfa içinde çalışan her adımın (sayfa bilgilerini okuma, bir axe çalıştırması) 60 saniyelik bir
  sınırı vardır. Zamanında yanıt vermeyen veya kontrol sırasında kendiliğinden başka bir adrese
  geçen sayfa "kontrol çalıştırılamadı" olarak kaydedilir.
- Tarama tarihleri: aşağıdaki "Tam tarama" bölümüne bakın.

## Sorumlu tarama

- **robots.txt:** Sayfadan önce, RFC 9309'a göre alınır. Ürün adımız `TurkiyeWebA11yReport` için
  yazılmış kurallar, yoksa `*` için yazılmış kurallar geçerlidir. Ana sayfaya izin verilmiyorsa
  veya robots.txt sunucu hatası döndürüyorsa sayfa yüklenmez ve site "atlandı (robots.txt)"
  olarak sayılır. Standartta belirtildiği gibi, robots.txt bulunmaması (örneğin HTTP 404)
  taramaya izin verir.
- **Hız sınırları:** Aynı anda en fazla 3 tarayıcı sekmesi (asla 4'ten fazla) ve hepsi birlikte en
  fazla 3 saniyede bir yeni site.
- **User-Agent:** `Mozilla/5.0 (compatible; TurkiyeWebA11yReport/<sürüm>; +<bu sayfanın bağlantısı>)`.
- **Bir site bizi engellerse dururuz.** HTTP 401, 403, 429 veya 503 (robots.txt'te ya da sayfada)
  ve reddedilen veya kesilen bağlantılar "erişilemedi" olarak kaydedilir; sayfa yüklenmez (veya
  yeniden yüklenmez) ve site aynı taramada tekrar denenmez. Proxy kullanmayız, IP adresi
  değiştirmeyiz, tarayıcıyı gizlemeyiz ve CAPTCHA çözmeyiz.
- **Tekrar denemeler:** Bir site, bir taramada en fazla bir kez ve yalnızca hiç sayfa alınamayan bir
  DNS veya başka bir ağ hatasından sonra tekrar denenir. Zaman aşımına düşen sayfalar tekrar
  denenmez, çünkü yavaş bir sayfa çoğu zaman yüzlerce kaynağı zaten yüklemiştir. Kamu siteleri
  asla tekrar denenmez.
- **Denetim kaydı:** Her tarama ve yarıda kalan bir taramanın her devamı; tarih ve saati, araç
  sürümlerini (tarayıcı uygulaması, .NET, Playwright, tarayıcı, axe-core), site listesinin
  sürümünü, User-Agent'ı ve tüm ayarları kaydeder. Her site için kaç istek yapıldığı kaydedilir:
  robots.txt istekleri, sayfa yüklemeleri (yönlendirmeler dahil) ve tarayıcının sayfa için
  yüklediği kaynaklar. Tarama kaydı bir JSON dosyası olarak yayımlanır. Bilinen eksik: Tarama
  durdurulduğunda o anda işlenmekte olan sitenin istekleri sayılmaz; bu site tarama devam
  ettiğinde yeniden taranır.

## Neleri saklıyoruz

Yalnızca teknik sonuçlar:

- axe kural kimliği, etkisi, WCAG etiketleri, etkilenen öğe sayısı
- kural ve görünüm başına en fazla 20 örnek öğe için CSS seçicisi ve axe'ın bildirdiği HTML
  kesiti (300 karaktere kısaltılmış); etkilenen öğelerin toplam sayısı her zaman saklanır
- sayfa bilgileri: yönlendirmelerden sonraki son adres, HTTP durumu, `lang` özelliğinin değeri,
  sayfa başlığının olup olmadığı (başlık metni değil), yer işareti sayısı, `main` yer işaretinin
  olup olmadığı, başlık sayısı, `h1` sayısı ve bir başlık düzeyinin atlanıp atlanmadığı
  (örneğin h2 → h4)

Sayfa metni, ekran görüntüsü, çerez veya kişisel veri **saklamayız**. Seçiciler ve HTML kesitleri
yerel veritabanımızda kalır ve **yayımlanmaz**; herkese açık site sorgulaması yalnızca kural
adlarını ve sayıları gösterir.

## Sayılar nasıl hesaplanır

- Bir **tespit edilen hata**, sayfadaki bir öğenin bir axe kuralından geçememesidir. Alternatif
  metni olmayan 12 görseli olan bir sayfada bu kuraldan 12 tespit edilen hata vardır.
- Masaüstü ve mobil görünüm için: en az bir hata tespit edilen ana sayfaların oranı ve sayfa
  başına ortalama (aritmetik ortalama) tespit edilen hata sayısı.
- Sektör karşılaştırmaları ve en sık görülen hata türleri masaüstü görünümü kullanır. Bir hata
  türünün oranı, o hatanın en az bir kez görüldüğü kontrol edilmiş ana sayfaların oranıdır.
- `lang` kullanımı (yok veya `tr` ya da `tr-TR` gibi Türkçe), `main` yer işaretinin olup olmadığı,
  yer işareti ve başlık sayıları, tam olarak bir veya hiç `h1` olmayan sayfalar ve bir başlık
  düzeyini atlayan sayfalar.
- Atlanan, erişilemeyen veya kontrol edilemeyen siteler ayrıca gösterilir ve **asla** hatasız
  sayılmaz. Kapsam dışı kalmayı isteyen siteler yayımlanan hiçbir sayıya dahil edilmez.
- Herkese açık rapor yalnızca sektör düzeyinde sayılar gösterir, asla bir site listesi veya
  sıralaması göstermez.

## WebAIM Million'dan farklarımız

WebAIM, WAVE motorunu kullanır; biz axe-core kullanıyoruz. İki motor kısmen farklı kuralları
kontrol eder (bkz. "WAVE ile elle karşılaştırma"), bu yüzden sayılarımız WebAIM'in sayılarıyla
doğrudan karşılaştırılamaz. Listemiz Türkiye ile sınırlıdır ve CrUX'tan gelir; sonuçları sektöre
göre veririz; ayrıca mobil görünümü de kontrol ederiz.

## Tam tarama (Kasım 2026)

Tam tarama (`2026-full`, tarama kaydı `data/runs/2026-full.json`), listedeki 1.000 siteyi 11 Kasım
2026'da Türkiye saatiyle 14.59 ile 18.18 arasında axe-core 4.14.0, Chromium 156 ve Playwright
1.64.1 ile kontrol etti.

| Sonuç | Site |
|---|---|
| Kontrol edildi | 878 |
| İsteği reddetti veya erişilemedi (57 × HTTP 403, 1 × 429, 1 × 503, 1 reddedilen bağlantı) | 60 |
| Zamanında yüklenmedi | 33 |
| Güvenli bağlantı (TLS) kurulamadı | 11 |
| Atlandı: robots.txt izin vermiyor | 5 |
| Kontrol çalıştırılamadı | 5 |
| Ağ hatası, HTTP hatası, alan adı bulunamadı (her biri 2) | 6 |
| Listedeki başka bir siteye yönleniyor | 2 |

İstekler: toplam 139.732 (1.028 robots.txt isteği, 1.026 sayfa yüklemesi, tarayıcının bu sayfalar
için yüklediği 137.678 kaynak).

Bu taramayla ilgili notlar:

- **Tarama bir kez yarıda kaldı.** 586. sitede bir sayfa, tarayıcımız onu okurken kendiliğinden
  başka bir adrese geçti ve yakalanmayan bir hata tarayıcıyı durdurdu. Sayfa içindeki her kontrol
  adımına bir zaman sınırı ekledik, bu tür sayfaları "kontrol çalıştırılamadı" olarak kaydettik
  ve taramaya kaldığı yerden devam ettik; bitmiş 586 site yeniden taranmadı. İki oturum ve
  ayarları tarama kaydında yer alır. Tarayıcı durduğunda işlenmekte olan birkaç sitenin istekleri
  sayılmadı; bu siteler devam ederken yeniden tarandı.
- **Kontrol edilen sayfaların %44,1'i**, sayfa hazır olduktan 10 saniye sonra ek içerik yüklemeyi
  (tarayıcının load olayı) bitirmemişti. Yine de kontrol edildiler; bkz. "Her sayfa nasıl
  taranır".
- **Ortalamalar ve uç değerler:** Ortalama, aritmetik ortalamadır. Sayfa başına tespit edilen hata
  sayısı çok değişkendir: kontrol edilen ana sayfaların yarısında 29 veya daha az (ortanca),
  %90'ında 113 veya daha az hata vardı; tek bir sayfada 5.911 hata vardı. 40 sayfada (%4,6) hiç
  hata yoktu.
- **Olası yer tutucu sayfalar:** Kontrol edilen 7 sayfa beşten az kaynak yükledi. Bunların bir
  kısmı gerçek ana sayfa yerine bir bot kontrolü veya bakım sayfası göstermiş olabilir. Bunu
  güvenilir biçimde ayırt edemediğimiz için kontrol edilmiş sayıldılar.

## Pilot (Kasım 2026)

Bir pilot tarama (`2026-pilot`, tarama kaydı `data/runs/2026-pilot.json`), 11 Kasım 2026'da listenin
ilk 50 sitesini axe-core 4.14.0, Chromium 156 ve Playwright 1.64.1 ile taradı.

| Sonuç | Site |
|---|---|
| Tarandı | 40 |
| 30 saniyede yüklenmedi | 6 |
| İsteği reddetti (HTTP 403) | 2 |
| Alan adı bulunamadı | 2 |

İstekler: toplam 9.808 (50 robots.txt isteği, 51 sayfa yüklemesi, tarayıcının bu sayfalar için
yüklediği 9.707 kaynak).

Yöntemi değiştiren bulgular:

- Pilotta bulunan iki tarayıcı hatası, sonuçlar kullanılmadan önce düzeltildi: tekrar denenen
  sitelerde ilk denemenin istek sayıları kayboluyordu ve kayıtlı kural sonuçları veritabanında
  sitelerine bağlanmamıştı (verinin kendisi sağlamdı ve yeniden bağlandı). İlk hata nedeniyle
  pilot tarama kaydı, tekrar denenen 9 sitenin isteklerini eksik sayar (DNS hatası dışında her biri
  için bir sayfa yüklemesi).
- 50 sayfanın 6'sı, içeriği hazır olduğu halde 30 saniye içinde tarayıcının load olayını
  tetiklemedi (150–1.000 kaynak yüklediler). Artık belgenin hazır olması beklenip load olayına 10
  saniye daha veriliyor (bkz. "Her sayfa nasıl taranır") ve zaman aşımları tekrar denenmiyor.
- 50 sitenin 2'si, CrUX'ta yalnızca bir uygulama alt alan adı (örneğin `uygulama.gtb.gov.tr`)
  listelendiği ve tahmin edilen `www.` ana sayfası olmadığı için başarısız oldu. Bu tür alan
  adları artık listeye alınmıyor; bu değişiklik 1.000 sitenin 42'sini değiştirdi. Bunlar arasında
  alt alan adları ana sayfalarından daha çok ziyaret edilen bazı üniversiteler ve kamu kurumları da
  var.

### WAVE ile elle karşılaştırma

Proje sahibi 11 Kasım 2026'da beş pilot siteyi WAVE tarayıcı eklentisiyle elle kontrol etti
(masaüstü tarayıcı penceresi, çerez bannerına dokunulmadan, bizim taramamızdaki gibi). Bizim
sayılarımız yaklaşık bir saat önce yapılan pilot taramadan (masaüstü görünüm) gelir.

| Site | WAVE hataları | WAVE kontrast hataları | Bizim başarısız kural / öğe | Bizim başlıca bulgularımız |
|---|---|---|---|---|
| milliyet.com.tr | 26 (form etiketi 16, boş bağlantı 8, alternatif metin 2) | 92 | 6 / 50 | kontrast 31, bağlantı adı 12, alternatif metin 3 |
| migros.com.tr | 1 (form etiketi 1) | 83 | 5 / 83 | kontrast 76, etiket/ad uyuşmazlığı 4 |
| yapikredi.com.tr | 11 (form etiketi 6, alternatif metin 2, boş bağlantı 2, boş düğme 1) | 0 | 7 / 23 | liste yapısı 12, ARIA rolleri 7, form etiketi 2 |
| metu.edu.tr | 3 (boş bağlantı 3) | 1 | 3 / 13 | dokunma hedefi 7, ARIA özelliği 5, çerçeve başlığı 1 |
| saglik.gov.tr | 0 | 2 | 0 / 0 | yok |

Öğrendiklerimiz:

- **Kontrast sonuçları beş sitenin dördünde uyuşuyor** (83'e 76, 0'a 0, 1'e 0, 2'ye 0). İstisna,
  içeriği iki kontrol arasında değişen bir haber sitesinin ana sayfası (92'ye 31).
- **İki araç farklı şeyleri kontrol eder.** axe; liste yapısını, gerekli ARIA rollerini, dokunma
  hedefi boyutunu (WCAG 2.2), etiket/ad uyuşmazlığını ve yakınlaştırmayı engelleyen
  `meta viewport` ayarını hata olarak bildirir. WAVE bunların bir kısmını kontrol etmez, bir
  kısmını da hata değil uyarı olarak gösterir.
- **WAVE, axe'ın bulmadığı bazı boş bağlantı, boş düğme, eksik alternatif metin ve form etiketi
  bildirdi.** Olası neden, WAVE'in bazı gizli içerikleri (menüler, iletişim kutuları, çerez
  ayarları) de değerlendirmesi, axe'ın ise yalnızca kullanıcılara gösterilen içeriği kontrol
  etmesidir. Bunu öğe öğe doğrulamadık.
- **Sıfır sonucu gerçek, engellenmiş bir sayfa değil:** saglik.gov.tr bizim taramamızda hiç hata
  vermedi, WAVE'de de neredeyse hiç vermedi.
- **Bu rapor için sonucu:** Sayılarımız axe-core'dan gelir ve WAVE motorunu kullanan WebAIM Million
  ile doğrudan karşılaştırılamaz. Sektörleri birbiriyle karşılaştırırız; ortalamalarımızı
  WebAIM'inkilerle karşılaştırmayız.

## Site sorgulama

[Site sorgulama sayfası](lookup.html), biri adresini yazdığında bir ana sayfanın sonucunu
gösterir: hangi otomatik kuralların başarısız olduğu, kaç öğenin etkilendiği ve sayfanın dil ve
başlık bilgileri. Yalnızca kural adlarını ve sayıları gösterir, sayfanın HTML'ini göstermez. Bir
site listesi veya sıralaması yoktur: her sonuç, alan adının SHA-256 özetiyle adlandırılmış bir
dosyada saklanır; bu yüzden bir sonuç yalnızca o alan adı yazılarak bulunabilir. Taranan alan
adlarının listesi, tekrarlanabilirlik için kaynak depoda yine herkese açıktır.

## Hata türlerinin Türkçe açıklamaları

Site sorgulaması, her hata türü için kısa bir Türkçe açıklama ve bir düzeltme örneği
gösterebilir.

- Kendi bilgisayarımızda çalışan bir dil modeli (açık `gemma3:4b` modeliyle Ollama), en az bir kez
  başarısız olan her axe kuralı için ilk taslağı yazdı.
- Modele **yalnızca axe-core'un kuralı kendi tanımı** (kural kimliği, özet, açıklama ve WCAG
  kriterleri) verildi. Taranan web sitelerinden hiçbir şey verilmedi.
- Küçük yerel modelin taslakları çoğu zaman teknik olarak yanlıştı (örneğin yanlış "hatalı" kod,
  birebir aynı "hatalı" ve "doğru" örnekler veya yanlış etkilenen kullanıcı grubu). Bu yüzden 27
  taslağın hepsi, axe-core'un kural tanımlarına dayanarak Claude (bir yapay zekâ asistanı)
  tarafından yeniden yazıldı ve her kayıt bunu notlarında belirtir.
- Proje sahibi her metni yayımlanmadan önce kontrol eder. Yalnızca onaylanmış açıklamalar
  yayımlanır; tüm sürümler `data/explanations/rules.tr.json` dosyasında saklanır.

## Kapsam dışı kalma {#opting-out}

Site sahipleri sitelerinin taranmamasını ve sonucunun yayımlanmamasını isteyebilir.

- **Nasıl:** Projenin GitHub deposunda bir
  [kapsam dışı kalma talebi](https://github.com/Yunuskngl/turkiye-web-accessibility-report/issues/new?template=opt-out.yml)
  açın. Yalnızca alan adı gerekir. GitHub talepleri herkese açıktır; lütfen kişisel veri
  eklemeyin.
- **Kanıt gerekmez:** Bir siteyi rapordan çıkarmanın zararı küçüktür; bu yüzden her talebi,
  sahiplik kanıtı istemeden kabul ederiz.
- **Sonrasında ne olur:** Alan adı `data/opt-out.csv` dosyasına eklenir. Sonraki taramadan itibaren
  taranmaz; bu sitenin bir sonraki yayımlanmasından itibaren sorgulama sonucu olmaz ve yayımlanan
  hiçbir sayıya dahil edilmez. Talepler elle işlendiği için değişiklik bir sonraki yayımlamada
  görünür.

## Veri kaynakları ve lisanslar

- Site listesi: Google'ın Chrome UX Report (CrUX) verisi, CC BY-SA 4.0 lisanslı
  (https://developer.chrome.com/docs/crux/), https://github.com/zakird/crux-top-lists adresindeki
  kopyalar üzerinden (commit `a67d0c8f09326df09739560bc43205b4f949f8e6`).
- Deque Systems'in axe-core'u, MPL-2.0 lisanslı: https://github.com/dequelabs/axe-core
