# LinkedIn paylaşım taslağı

> Taslaktır. Yayımlamadan önce kendi üslubuna göre düzenle. Site adı verme; sonuçlar sektör düzeyinde.

---

Türkiye'de en çok ziyaret edilen 1.000 web sitesinin ana sayfasını erişilebilirlik açısından
otomatik olarak kontrol ettim. 🔍

Kontrol edebildiğim 878 ana sayfanın **%95'inde** en az bir erişilebilirlik hatası çıktı.

En sık görülenler:
• Düşük kontrastlı metin: sayfaların %76'sı
• Metni olmayan bağlantılar: %65
• Alternatif metni olmayan görseller: %45
• Çok küçük dokunma hedefleri: %45

Sayfaların dörtte biri mobilde yakınlaştırmayı engelliyor. Bu, az gören kullanıcılar için metni
okunamaz hale getirebiliyor.

Sektörlere göre ana sayfa başına ortalama hata sayısı: kamuda 29,7, bankacılık ve finansta 34,4,
üniversitelerde 41,6, haber ve medyada 61,2, e-ticarette 67,0.

Önemli not: Bu otomatik bir kontrol, yasal bir denetim değil. Otomatik araçlar sorunların yalnızca
bir kısmını bulur; hata çıkmaması sitenin erişilebilir olduğu anlamına gelmez. Raporda site
sıralaması ya da isim yok.

Projeyi C# / .NET 10 ile, öğrenerek geliştirdiğim bir portföy çalışması olarak yaptım:
Playwright ve axe-core ile tarama, EF Core ve SQLite ile kayıt, statik bir rapor sitesi. Taramada
robots.txt'ye uydum, her siteye tek sayfa yüklemesi yaptım ve beni engelleyen sitelerde tekrar
denemedim. Rapor sitesinin kendisini de aynı araçla kontrol ettim: sıfır tespit edilen hata.

📊 Rapor: https://yunuskngl.github.io/turkiye-web-accessibility-report/tr/
💻 Kod ve yöntem: https://github.com/Yunuskngl/turkiye-web-accessibility-report

Kendi sitenizin sonucunu sorgulama sayfasından görebilirsiniz.

#erişilebilirlik #a11y #WCAG #dotnet #csharp #webgeliştirme
