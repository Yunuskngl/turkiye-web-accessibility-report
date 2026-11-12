namespace A11yReport.Site.Html;

/// <summary>The English and Turkish texts of the site. English pages are at the root, Turkish pages in /tr/.</summary>
public static class SiteLanguages
{
    public static SiteTexts English { get; } = new()
    {
        LangCode = "en",
        Folder = "",
        Format = SiteFormat.English,
        SiteName = "Türkiye Web Accessibility Report",
        SkipLink = "Skip to main content",
        NavLabel = "Main",
        NavReport = "Results",
        NavLookup = "Check a website",
        NavMethodology = "How we did it",
        OtherLanguageName = "Türkçe",
        LanguageName = "English",
        LanguageLabel = "Language",
        FooterText = "Results of an automatic check. This is not a legal audit. Source code and data:",
        FooterRepository = "GitHub",
        FooterOptOut = "Website owners can <a href=\"methodology.html#opting-out\">ask to be left out</a>.",

        ReportTitle = "How easy are Türkiye's most visited websites to use for people with disabilities?",
        ReportLede = "We automatically checked the home pages of {pages} popular websites in Türkiye. We looked for problems "
            + "that make a site hard to use for people who, for example, use a screen reader, cannot use a mouse or have low vision. "
            + "Checked on {date}.",
        ReportPilotNotice = "<strong>Pilot results.</strong> These numbers come from a small test run. They are not the final report.",
        KeyNumbersHeading = "The results at a glance",
        StatDesktopShare = "of home pages had at least one problem on a computer screen",
        StatDesktopAverage = "problems per home page on average, on a computer screen",
        StatMobileShare = "of home pages had at least one problem on a phone screen",
        StatMobileAverage = "problems per home page on average, on a phone screen",
        FailureDefinition = "<strong>What counts as a problem?</strong> One part of a page that fails one automatic check, for example "
            + "one image without a text description. Automatic checks find only some of the barriers people meet, so a page with "
            + "no problems found is not automatically easy for everyone to use.",
        SectorsHeading = "How do sectors compare?",
        SectorChartTitle = "Average number of problems per home page, by sector",
        SectorChartDescription = "Bar chart of the average number of problems per home page in each sector, on a computer screen. The same numbers are in the table below.",
        SectorChartAxis = "Average problems per home page (computer screen)",
        SectorTableCaption = "Problems by sector (computer screen)",
        SectorTableHeaders = ["Sector", "Sites in the list", "Home pages checked", "Pages with at least one problem", "Average problems per page"],
        RulesHeading = "The most common problems",
        RuleChartTitle = "The most common problems",
        RuleChartDescription = "Bar chart of how many of the checked home pages have each problem, on a computer screen. The same numbers are in the table below.",
        RuleChartAxis = "Share of checked home pages (computer screen)",
        RuleTableCaption = "The most common problems (computer screen)",
        RuleTableHeaders = ["Problem (technical name)", "Share of pages", "Pages", "Times found"],
        StructureHeading = "Is the page structure clear?",
        StructureCaption = "Language and structure of the home pages",
        StructureHeaders = ["What we looked at", "Home pages"],
        StructureRows =
        [
            "Page language not set, so screen readers may read Turkish text with the wrong pronunciation",
            "Page language set to Turkish",
            "The main content area is marked, so it can be found quickly",
            "Exactly one main heading (h1)",
            "No main heading (h1)",
            "Heading levels are skipped, which makes the outline harder to follow",
            "Average number of marked page areas (such as menu, main content, footer)",
            "Average number of headings",
        ],
        CoverageHeading = "Which websites could we check?",
        CoverageText = "{listed} websites were in the list. Websites we could not check are listed here. They are "
            + "<strong>not</strong> counted as websites without problems.",
        CoverageCaption = "What happened with each website in the list",
        CoverageHeaders = ["Result", "Websites"],
        CoverageChecked = "Checked",
        AboutHeading = "About this report",
        AboutText = "<p>The report shows results by sector only. It does not rank or name websites, and it is not a legal audit. "
            + "The numbers come from one tool (axe-core) and cannot be compared directly with reports that use other tools.</p>"
            + "<p>Want to see the result for one website? Use <a href=\"lookup.html\">Check a website</a>. "
            + "How we chose and checked the sites is explained on <a href=\"methodology.html\">How we did it</a>.</p>",

        LookupTitle = "Check a website",
        LookupLede = "Type a website address to see the result of our automatic check of its home page. We show a site's result "
            + "only when someone looks for it; we do not publish a list or a ranking.",
        LookupPilotNotice = "<strong>Pilot results.</strong> Only the websites in the small test run can be found.",
        LookupLabel = "Website address",
        LookupHint = "For example <code>example.com.tr</code>",
        LookupButton = "Show result",
        LookupNoScript = "This search needs JavaScript. All numbers are also on the results page.",
        LookupReadFirstHeading = "Good to know",
        LookupReadFirst = "<li>Only the home page was checked, on {date}.</li>"
            + "<li>Automatic checks find only some problems. If no problems are found, the site may still be hard to use for some people.</li>"
            + "<li>This is not a legal audit.</li>"
            + "<li>Website owners can <a href=\"methodology.html#opting-out\">ask to be left out</a>.</li>",

        Script = new Dictionary<string, string>
        {
            ["enterAddress"] = "Please type a website address, for example example.com.tr.",
            ["lookingUp"] = "Looking for {domain}…",
            ["noResult"] = "No result for {domain}. It was not in our list, or its owner asked to be left out.",
            ["failed"] = "Something went wrong. Please try again.",
            ["resultFor"] = "Result for {domain}",
            ["sectorLine"] = "Sector: {sector}. Checked on {date} with the axe-core {axe} tool.",
            ["notChecked"] = "This home page was not checked: {status}. It is not counted as a page without problems.",
            ["summary"] = "Summary",
            ["view"] = "Screen",
            ["failedRules"] = "Types of problems",
            ["detectedFailures"] = "Problems found",
            ["Desktop"] = "On a computer screen",
            ["Mobile"] = "On a phone screen",
            ["rulesCaption"] = "{view}: problems found",
            ["ruleColumn"] = "Problem (technical name)",
            ["criteriaColumn"] = "WCAG rule",
            ["elementsColumn"] = "Times found",
            ["noFailures"] = "No problems found by the automatic check. This does not mean the page is easy for everyone to use.",
            ["factsCaption"] = "Language and structure",
            ["measure"] = "What we looked at",
            ["value"] = "Result",
            ["lang"] = "Page language",
            ["hasTitle"] = "Has a page title",
            ["hasMain"] = "Main content area is marked",
            ["h1Count"] = "Main headings (h1)",
            ["headingCount"] = "Headings in total",
            ["skips"] = "Heading levels skipped",
            ["yes"] = "Yes",
            ["no"] = "No",
            ["notSet"] = "Not set",
            ["stillLoading"] = "The page was still loading extra content (for example ads) when we checked it.",
        },
    };

    public static SiteTexts Turkish { get; } = English with
    {
        LangCode = "tr",
        Folder = "tr",
        Format = SiteFormat.Turkish,
        SiteName = "Türkiye Web Erişilebilirlik Raporu",
        SkipLink = "Ana içeriğe geç",
        NavLabel = "Ana menü",
        NavReport = "Sonuçlar",
        NavLookup = "Site sorgula",
        NavMethodology = "Nasıl yaptık?",
        OtherLanguageName = "English",
        LanguageName = "Türkçe",
        LanguageLabel = "Dil",
        FooterText = "Otomatik bir kontrolün sonuçlarıdır. Yasal denetim değildir. Kaynak kod ve veriler:",
        FooterRepository = "GitHub",
        FooterOptOut = "Site sahipleri <a href=\"methodology.html#opting-out\">rapordan çıkarılmayı</a> isteyebilir.",

        ReportTitle = "Türkiye'nin en çok ziyaret edilen siteleri engelli kullanıcılar için ne kadar kolay?",
        ReportLede = "Türkiye'de popüler {pages} web sitesinin ana sayfasını otomatik olarak kontrol ettik. Örneğin ekran okuyucu "
            + "kullanan, fare kullanamayan veya az gören kişiler için siteyi zorlaştıran sorunlara baktık. Kontrol tarihi: {date}.",
        ReportPilotNotice = "<strong>Pilot sonuçları.</strong> Bu sayılar küçük bir deneme taramasından gelir. Nihai rapor değildir.",
        KeyNumbersHeading = "Bir bakışta sonuçlar",
        StatDesktopShare = "ana sayfada bilgisayar ekranında en az bir sorun bulundu",
        StatDesktopAverage = "ana sayfa başına ortalama sorun (bilgisayar ekranı)",
        StatMobileShare = "ana sayfada telefon ekranında en az bir sorun bulundu",
        StatMobileAverage = "ana sayfa başına ortalama sorun (telefon ekranı)",
        FailureDefinition = "<strong>Sorun ne demek?</strong> Sayfanın bir parçasının bir otomatik kontrolden geçememesi; örneğin "
            + "metin açıklaması olmayan bir görsel. Otomatik kontroller insanların karşılaştığı engellerin yalnızca bir kısmını bulur. "
            + "Bu yüzden sorun bulunmayan bir sayfa herkes için kolay demek değildir.",
        SectorsHeading = "Sektörler nasıl?",
        SectorChartTitle = "Sektörlere göre ana sayfa başına ortalama sorun",
        SectorChartDescription = "Her sektörde ana sayfa başına ortalama sorun sayısını gösteren çubuk grafik, bilgisayar ekranı. Aynı sayılar aşağıdaki tabloda da var.",
        SectorChartAxis = "Ana sayfa başına ortalama sorun (bilgisayar ekranı)",
        SectorTableCaption = "Sektörlere göre sorunlar (bilgisayar ekranı)",
        SectorTableHeaders = ["Sektör", "Listedeki site", "Kontrol edilen ana sayfa", "En az bir sorunu olan sayfa", "Sayfa başına ortalama sorun"],
        RulesHeading = "En sık görülen sorunlar",
        RuleChartTitle = "En sık görülen sorunlar",
        RuleChartDescription = "Kontrol edilen ana sayfaların ne kadarında her sorunun görüldüğünü gösteren çubuk grafik, bilgisayar ekranı. Aynı sayılar aşağıdaki tabloda da var.",
        RuleChartAxis = "Kontrol edilen ana sayfaların oranı (bilgisayar ekranı)",
        RuleTableCaption = "En sık görülen sorunlar (bilgisayar ekranı)",
        RuleTableHeaders = ["Sorun (teknik adı)", "Sayfa oranı", "Sayfa", "Kaç kez bulundu"],
        StructureHeading = "Sayfa düzeni anlaşılır mı?",
        StructureCaption = "Ana sayfaların dili ve düzeni",
        StructureHeaders = ["Neye baktık", "Ana sayfalar"],
        StructureRows =
        [
            "Sayfa dili belirtilmemiş; ekran okuyucular Türkçe metni yanlış telaffuzla okuyabilir",
            "Sayfa dili Türkçe olarak belirtilmiş",
            "Ana içerik alanı işaretlenmiş; böylece hızlıca bulunabiliyor",
            "Tam olarak bir ana başlık (h1) var",
            "Ana başlık (h1) yok",
            "Başlık düzeyleri atlanıyor; bu, sayfanın planını takip etmeyi zorlaştırır",
            "Ortalama işaretlenmiş sayfa bölümü (menü, ana içerik, alt bilgi gibi)",
            "Ortalama başlık sayısı",
        ],
        CoverageHeading = "Hangi siteleri kontrol edebildik?",
        CoverageText = "Listede {listed} web sitesi vardı. Kontrol edemediğimiz siteler burada gösteriliyor. Bu siteler "
            + "sorunsuz site olarak <strong>sayılmadı</strong>.",
        CoverageCaption = "Listedeki her sitenin durumu",
        CoverageHeaders = ["Durum", "Site"],
        CoverageChecked = "Kontrol edildi",
        AboutHeading = "Bu rapor hakkında",
        AboutText = "<p>Rapor yalnızca sektörlere göre sonuçları gösterir. Siteleri sıralamaz, isim vermez ve yasal bir denetim değildir. "
            + "Sayılar tek bir araçtan (axe-core) gelir; başka araç kullanan raporlarla doğrudan karşılaştırılamaz.</p>"
            + "<p>Bir sitenin sonucunu görmek için <a href=\"lookup.html\">Site sorgula</a> sayfasını kullanın. "
            + "Siteleri nasıl seçtiğimizi ve kontrol ettiğimizi <a href=\"methodology.html\">Nasıl yaptık?</a> sayfasında anlatıyoruz.</p>",

        LookupTitle = "Bir siteyi sorgula",
        LookupLede = "Bir web sitesi adresi yazın; ana sayfasının otomatik kontrol sonucunu görün. Bir sitenin sonucunu yalnızca "
            + "biri o siteyi aradığında gösteriyoruz; liste veya sıralama yayımlamıyoruz.",
        LookupPilotNotice = "<strong>Pilot sonuçları.</strong> Yalnızca küçük deneme taramasındaki siteler bulunabilir.",
        LookupLabel = "Web sitesi adresi",
        LookupHint = "Örneğin <code>ornek.com.tr</code>",
        LookupButton = "Sonucu göster",
        LookupNoScript = "Bu arama için JavaScript gerekir. Tüm sayılar sonuçlar sayfasında da var.",
        LookupReadFirstHeading = "Bilmeniz gerekenler",
        LookupReadFirst = "<li>Yalnızca ana sayfa, {date} tarihinde kontrol edildi.</li>"
            + "<li>Otomatik kontroller sorunların yalnızca bir kısmını bulur. Sorun bulunmasa bile site bazı kişiler için zor olabilir.</li>"
            + "<li>Bu bir yasal denetim değildir.</li>"
            + "<li>Site sahipleri <a href=\"methodology.html#opting-out\">rapordan çıkarılmayı</a> isteyebilir.</li>",

        Script = new Dictionary<string, string>
        {
            ["enterAddress"] = "Lütfen bir web sitesi adresi yazın, örneğin ornek.com.tr.",
            ["lookingUp"] = "{domain} aranıyor…",
            ["noResult"] = "{domain} için sonuç yok. Site listemizde değildi veya sahibi rapordan çıkarılmayı istedi.",
            ["failed"] = "Bir şeyler ters gitti. Lütfen tekrar deneyin.",
            ["resultFor"] = "{domain} için sonuç",
            ["sectorLine"] = "Sektör: {sector}. Kontrol tarihi: {date}, kullanılan araç: axe-core {axe}.",
            ["notChecked"] = "Bu ana sayfa kontrol edilemedi: {status}. Sorunsuz sayfa olarak sayılmaz.",
            ["summary"] = "Özet",
            ["view"] = "Ekran",
            ["failedRules"] = "Sorun türü",
            ["detectedFailures"] = "Bulunan sorun",
            ["Desktop"] = "Bilgisayar ekranında",
            ["Mobile"] = "Telefon ekranında",
            ["rulesCaption"] = "{view}: bulunan sorunlar",
            ["ruleColumn"] = "Sorun (teknik adı)",
            ["criteriaColumn"] = "WCAG kuralı",
            ["elementsColumn"] = "Kaç kez bulundu",
            ["noFailures"] = "Otomatik kontrol sorun bulmadı. Bu, sayfanın herkes için kolay olduğu anlamına gelmez.",
            ["factsCaption"] = "Dil ve düzen",
            ["measure"] = "Neye baktık",
            ["value"] = "Sonuç",
            ["lang"] = "Sayfa dili",
            ["hasTitle"] = "Sayfa başlığı var",
            ["hasMain"] = "Ana içerik alanı işaretlenmiş",
            ["h1Count"] = "Ana başlık (h1) sayısı",
            ["headingCount"] = "Toplam başlık",
            ["skips"] = "Başlık düzeyi atlanıyor",
            ["yes"] = "Evet",
            ["no"] = "Hayır",
            ["notSet"] = "Belirtilmemiş",
            ["stillLoading"] = "Sayfa, kontrol ettiğimizde hâlâ ek içerik (örneğin reklam) yüklüyordu.",
        },
    };

    public static IReadOnlyList<SiteTexts> All { get; } = [English, Turkish];
}
