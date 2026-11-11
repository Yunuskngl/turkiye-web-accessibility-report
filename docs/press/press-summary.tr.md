# Basın özeti: Türkiye Web Erişilebilirlik Raporu 2026

**Türkiye'de en çok ziyaret edilen web sitelerinin ana sayfalarının %95'inde otomatik olarak
tespit edilebilen erişilebilirlik hataları bulundu.**

Türkiye Web Erişilebilirlik Raporu, Türkiye'de en çok ziyaret edilen 1.000 web sitesinin ana
sayfasını 11 Kasım 2026'da otomatik olarak kontrol etti. Ana sayfaların 878'i kontrol edilebildi.

## Temel bulgular

- Kontrol edilen ana sayfaların **%95,4'ünde** en az bir erişilebilirlik hatası tespit edildi
  (masaüstü görünüm; mobil görünümde %95,1).
- Ana sayfa başına ortalama **58,8** hata tespit edildi (mobilde 55,3). Sayfaların yarısında 29 veya
  daha az hata vardı; birkaç sayfadaki çok yüksek sayılar ortalamayı yukarı çekiyor.
- **En sık görülen sorunlar:** düşük kontrastlı metin (sayfaların %75,7'si), metinsiz bağlantılar
  (%64,6), alternatif metni olmayan görseller (%45,3) ve çok küçük dokunma hedefleri (%44,9).
- Sayfaların **%26,7'si** mobil cihazlarda yakınlaştırmayı engelliyor.
- Sayfaların **%90,9'unda** sayfa dili Türkçe olarak belirtilmiş; **%45,3'ünde**
  birinci düzey başlık (h1) yok.

## Sektörlere göre (ana sayfa başına ortalama tespit edilen hata, masaüstü)

| Sektör | Kontrol edilen sayfa | Ortalama hata |
|---|---|---|
| Kamu | 92 | 29,7 |
| Bankacılık ve finans | 50 | 34,4 |
| Üniversiteler | 51 | 41,6 |
| Haber ve medya | 167 | 61,2 |
| E-ticaret | 234 | 67,0 |
| Diğer | 284 | 67,6 |

## Önemli sınırlar

- Bu bir **otomatik** kontroldür ve yasal uygunluk denetimi değildir. Otomatik araçlar
  erişilebilirlik engellerinin yalnızca bir kısmını bulur; hata tespit edilmemesi sitenin
  erişilebilir olduğu anlamına gelmez.
- Yalnızca ana sayfalar kontrol edildi. Rapor siteleri sıralamaz ve site adı vermez; sonuçlar
  sektör düzeyindedir.
- Sonuçlar axe-core aracıyla elde edildi ve farklı araç kullanan raporlarla (örneğin WebAIM
  Million) doğrudan karşılaştırılamaz.
- 122 site kontrol edilemedi (60'ı isteği reddetti, 33'ü zamanında yüklenmedi, diğerleri teknik
  hatalar); bu siteler hatasız sayılmadı.

## Yöntem kısaca

Site listesi: Chrome UX Report (CrUX) Türkiye verisi, Ekim 2026. Araç: axe-core 4.14.0, WCAG 2.2 A ve
AA kuralları, masaüstü (1280×800) ve mobil (390×844) görünüm. Her siteye tek sayfa yüklemesi
yapıldı, robots.txt kurallarına uyuldu ve bizi engelleyen sitelerde tekrar denenmedi.

## Bağlantılar

- Rapor: https://yunuskngl.github.io/turkiye-web-accessibility-report/tr/
- Yöntem: https://yunuskngl.github.io/turkiye-web-accessibility-report/tr/methodology.html
- Kaynak kod ve veriler: https://github.com/Yunuskngl/turkiye-web-accessibility-report
