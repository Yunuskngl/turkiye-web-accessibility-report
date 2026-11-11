using System.Text.Json;
using A11yReport.Core.Results;
using Microsoft.Playwright;

namespace A11yReport.Scanner.Scanning;

/// <summary>Reads structural facts from the DOM. Returns counts and flags only, never page text.</summary>
internal static class PageFactsReader
{
    private const int MaxLangLength = 35;

    // Headings: h1-h6 and role=heading with aria-level. Landmarks: HTML landmark elements and ARIA
    // landmark roles; header/footer only count at page level, not inside sectioning content.
    private const string Script = """
        () => {
          const levelOf = h => h.hasAttribute('aria-level')
            ? parseInt(h.getAttribute('aria-level'), 10)
            : parseInt(h.tagName.substring(1), 10);
          const levels = [...document.querySelectorAll('h1,h2,h3,h4,h5,h6,[role="heading"][aria-level]')]
            .map(levelOf).filter(l => l >= 1 && l <= 6);
          const skips = levels.some((l, i) => i > 0 && l > levels[i - 1] + 1);
          const landmarkSelector = 'main,nav,aside,header,footer,search,form[aria-label],form[aria-labelledby],' +
            'section[aria-label],section[aria-labelledby],[role=main],[role=navigation],[role=complementary],' +
            '[role=banner],[role=contentinfo],[role=search],[role=region],[role=form]';
          const nested = el => (el.tagName === 'HEADER' || el.tagName === 'FOOTER') && !el.hasAttribute('role')
            && el.parentElement && el.parentElement.closest('article,aside,main,nav,section');
          const landmarks = [...document.querySelectorAll(landmarkSelector)].filter(el => !nested(el));
          const lang = document.documentElement.getAttribute('lang');
          return {
            lang: lang === null ? null : lang.trim(),
            hasTitle: document.title.trim().length > 0,
            landmarkCount: landmarks.length,
            hasMain: document.querySelector('main,[role=main]') !== null,
            headingCount: levels.length,
            h1Count: levels.filter(l => l === 1).length,
            skipsHeadingLevel: skips
          };
        }
        """;

    public static async Task<PageFacts> ReadAsync(IPage page)
    {
        var json = await page.EvaluateAsync<JsonElement>(Script);
        return new PageFacts(
            ReadLang(json),
            json.GetProperty("hasTitle").GetBoolean(),
            json.GetProperty("landmarkCount").GetInt32(),
            json.GetProperty("hasMain").GetBoolean(),
            json.GetProperty("headingCount").GetInt32(),
            json.GetProperty("h1Count").GetInt32(),
            json.GetProperty("skipsHeadingLevel").GetBoolean());
    }

    private static string? ReadLang(JsonElement json)
    {
        var lang = json.GetProperty("lang");
        if (lang.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = lang.GetString() ?? string.Empty;
        return value.Length <= MaxLangLength ? value : value[..MaxLangLength];
    }
}
