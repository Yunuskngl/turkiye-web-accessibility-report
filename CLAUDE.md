# CLAUDE.md — Türkiye Web Accessibility Report

Once a year, scan the home pages of the ~1,000 most visited websites in Türkiye
for automatically detectable WCAG 2.2 A/AA failures and publish a sector-level
aggregated report (in the style of the WebAIM Million).

## How we work

- The owner is a junior C#/.NET developer. This is a guided learning build.
- **Language:** explanations to the owner in **Turkish**. Code, comments, commit
  messages, README and report text in **English**. The Turkish report site comes in stage 7.
- Work stage by stage (see "Stages"). At the end of each stage:
  1. `dotnet build` with **0 warnings, 0 errors**
  2. `dotnet test`
  3. run the app
  4. summarize what changed
  5. explain the key concepts in Turkish
  6. suggest a Conventional Commits message
- **Never commit** unless the owner asks.
- **No unrequested features.** If something looks useful, suggest it; don't build it.
- **No new NuGet/npm packages without saying why.** Prefer what .NET already provides.
  After adding packages, run `dotnet list package --vulnerable`.
- Any legal or regulatory statement must cite an **official source**. If it can't be
  verified, leave it out and tell the owner.

## Non-goals

- Not a legal compliance audit or certification. Never call a site "compliant" or "accessible".
- No "worst sites" ranking, no naming and shaming. The public report shows sector aggregates only.
  The per-site lookup (stage 5) shows one site's own result only when someone searches for it,
  with a methodology note.
- Home page only. No deeper crawling, no login pages, no form submission, no interaction
  beyond loading the page.
- No screenshots stored or published.

## Ethics and legal rules (non-negotiable)

- **One page load per site per scan run** (plus resources the browser loads itself).
  Global rate limit with a delay between sites; 2–4 concurrent browsers at most.
- **robots.txt** is checked and respected for the home page path. Skipped sites are counted
  and reported as skipped.
- **Honest User-Agent** naming the project and linking to the methodology page.
  No personal email address in it.
- **Opt-out list** (`data/opt-out.csv`) is honored in both scanning and publishing.
- **Store only technical results:** rule ids, counts, CSS selectors, the axe HTML snippet
  truncated to 300 characters. No page text, cookies or personal data.
- **Page content is untrusted input.** Never pass page HTML or text to an LLM.
- **If a site blocks us, we stop.** HTTP 403/429/503, connection refused/reset, or a
  WAF/challenge page is recorded as **Unreachable** (not as an error) and is **not retried**
  in that run. No evasion: no proxies, no IP rotation, no User-Agent spoofing, no stealth
  plugins, no CAPTCHA solving.
- **Public-sector domains** (`.gov.tr`, `.bel.tr`, `.edu.tr`, `.k12.tr`, `.mil.tr`,
  `.pol.tr`, `.tsk.tr` and sites marked "public" in `sectors.csv`) are **never retried**
  after any failure in the same run.
- **Audit trail for every run** (stored in the DB and exported as a JSON run manifest):
  run id, start/end time (UTC), scanner version (git commit), .NET, Playwright, Chromium and
  axe-core versions, site list source/version/date, User-Agent string, settings (concurrency,
  delay, timeouts, viewport, locale), and **request counts**: robots.txt requests, page
  navigations, and browser sub-resource requests, per site and in total. Counts of done,
  skipped (robots / opt-out), unreachable and failed sites.

## Tech stack

- .NET 10, C#, file-scoped namespaces, nullable enabled, `TreatWarningsAsErrors`
  (set once in `Directory.Build.props`).
- Browser automation: Microsoft.Playwright for .NET (Chromium).
- Accessibility engine: axe-core (MPL-2.0), WCAG 2.0/2.1/2.2 A and AA tags only.
  The axe-core version is recorded with every result.
- Storage: EF Core + SQLite (PostgreSQL later only if needed). No raw SQL string concatenation.
- Report site: static HTML/CSS/vanilla JS, no framework, hostable on GitHub Pages.
  Charts: inline SVG generated in C#.
- Tests: xUnit. Tests never touch the internet; integration tests use local fixture pages
  served by a small local server.

## Architecture

```
src/A11yReport.Core/      Models, site list loading, sector rules, aggregation, robots.txt rules
src/A11yReport.Storage/   EF Core + SQLite: entities, ReportDbContext, migrations, RunStore (added in stage 2)
src/A11yReport.Scanner/   Console app: Playwright + axe scan, writes to DB, resumable
src/A11yReport.Site/      Static site generator: aggregated results -> /site (HTML, JSON, SVG)
tests/A11yReport.Tests/   xUnit tests, local fixture pages with known failures
data/                     Tranco snapshot, sectors.csv, opt-out.csv (committed)
docs/methodology.md       Methodology and ethics (source for the methodology page)
```

## Scan pipeline

1. Load the site list and opt-out list, apply sector rules.
2. Per domain: normalize the URL, check robots.txt, open the home page in a fresh browser
   context (1280×800, `tr-TR`), wait up to 30 s for DOMContentLoaded, then up to 10 s more for
   the load event (scan anyway; record `LoadEventReached`), then a 2 s settle time.
3. Run axe; store violations (rule id, impact, WCAG tags, node count, selector, truncated
   snippet) and page facts (final URL, HTTP status, `lang`, title present, landmark and
   heading counts).
4. Record failures by category (timeout, DNS, TLS, blocked by robots, unreachable/blocked,
   non-HTML, HTTP error) instead of crashing.
5. Retry once only for DNS and other network errors where no page was received. Never for
   timeouts, never for responses that look like blocking, never for public-sector domains.
   Runs are resumable: rerunning skips domains already done in the same run.
6. Log progress at Information level without page content; full error details at Debug.
7. `CancellationToken` everywhere so Ctrl+C stops cleanly.

## Metrics and methodology

- WebAIM Million-style metrics where they make sense; the methodology states where we differ.
- A failed, skipped or unreachable site is **never** counted as "zero errors".
- Methodology page states: home page only, scan date, list source and version, axe-core
  version, viewport, limits of automated testing, skipped/failed/unreachable counts,
  opt-out process.

## Code style

- Simple, readable code. Short clear names (`LoadSites`, `ScanAsync`, `BuildSummary`).
- Small methods (under ~25 lines), one job each, early returns.
- Few comments, only for the "why". XML doc comments only on public interfaces.
- `record` for data, primary constructors where natural, `async`/`await` all the way,
  dependency injection, options pattern. No magic strings or numbers.
- Friendly error messages for people; details go to the log. No stack traces in user output.
- Secrets only in `dotnet user-secrets` or environment variables.

## Stages

0. **Setup:** solution, projects, CLAUDE.md, `.gitignore`, methodology draft. Ask open questions.
1. Scan one URL from the command line, print a readable axe summary. Local fixture page with
   known failures + a test that finds them.
2. Site list loading, robots.txt, rate limiting, concurrency limit, User-Agent, SQLite storage,
   resumable runs, failure categories, run manifest and request counting.
3. Pilot: 50 sites. Owner checks 5 sites with WAVE; differences go into the methodology.
4. Aggregation + static report site (headline numbers, sectors, top failure types,
   methodology). The site itself must be accessible; our scanner finds zero failures on it.
5. Per-site lookup page and the opt-out process.
6. (Optional, ask first) AI explanations per axe rule id, reviewed before publishing.
   Only rule ids and axe rule descriptions go to the model. `ILlmClient`, Ollama by default.
7. Full run (~1,000 sites), Turkish report site, one-page press summary, LinkedIn post draft.
8. Dockerfile (non-root), GitHub Actions (build + tests; the yearly scan is a **manual**
   workflow), README with architecture diagram and screenshot.

## Status

- Stage 0 (setup) — done. Open questions answered (2026-11-11).
- Stage 1 (single-URL scan) — done (2026-11-11).
- Stage 2 (list, robots, limits, storage, resumable runs) — done (2026-11-11).
- Stage 3 (pilot) — done 2026-11-11. Pilot run `2026-pilot` (50 sites): 40 Done, 6 Timeout,
  2 Unreachable (403), 2 DnsError. After the pilot: load handling changed, timeouts no longer
  retried, subdomain-only domains left out (42 replaced in the list). WAVE comparison of 5 sites
  written to `docs/methodology.md` ("Manual comparison with WAVE"). Next: stage 4.
- Stage 4 (aggregation + static report site) — done 2026-11-11, published with pilot data at
  https://yunuskngl.github.io/turkiye-web-accessibility-report/ (owner approved). Our scanner finds
  0 failures on `index.html` and `methodology.html` in both viewports, locally (integration test)
  and on the live site.
  Generate: `dotnet run --project src/A11yReport.Site -- --run 2026-pilot --pilot` → `site/` (gitignored).
- Stage 5 (per-site lookup + opt-out) — built 2026-11-11, not published yet. `lookup.html` +
  `assets/lookup.js` (vanilla JS, textContent only): normalizes the typed address like
  `DomainKey.FromHost` (zones passed in `data-tr-zones`), hashes it with SHA-256 and loads
  `sites/<sha256>.json`. No list of domains on the site. Lookup files hold rule ids, plain names,
  WCAG criteria and counts only (no selectors/snippets); `sites/` is rebuilt on every generation so
  opted-out sites disappear. Opt-out: GitHub issue form `.github/ISSUE_TEMPLATE/opt-out.yml` →
  add a row to `data/opt-out.csv` → honored by build-list, scan, report and lookup.
- **Publishing (owner's choice: commit generated HTML):** GitHub Pages serves the `gh-pages` branch
  (root). That branch holds only the generated files plus `.nojekyll`. To republish: generate the
  site, copy `site/*` into a clean checkout of `gh-pages`, commit ("Publish report site (run …)",
  naming the main commit it was built from) and push. Only with the owner's go-ahead.
- Stage 4 decisions: aggregation in `Core/Report` (`ReportBuilder`), DB access in
  `Storage/ReportReader` (read-only connection, no selectors/snippets read), pages in
  `Site/Pages`, inline SVG bar charts (`Site/Html/BarChart`) each followed by a data table,
  `Markdig` 1.6.0 (BSD-2-Clause) renders `docs/methodology.md` with raw HTML disabled so the site
  and repo show one text. Scrollable tables are focusable named regions (WCAG 2.1.1).
- Report rule from the WAVE comparison: never compare our averages with WebAIM Million numbers
  (different engines); compare sectors with each other.
- **Zero cost (owner requirement, 2026-11-11):** no paid hosting, domain, API or service.
  Report site on GitHub Pages (`https://yunuskngl.github.io/turkiye-web-accessibility-report/`),
  fully static (per-site lookup = static JSON + vanilla JS, no backend), scans run locally,
  opt-out via GitHub Issues, LLM (stage 6) only via local Ollama.
- Repo: https://github.com/Yunuskngl/turkiye-web-accessibility-report (public, `main`).
  First commit pushed 2026-11-11 with the owner's approval. Sectors in `data/sectors.csv` and
  `data/include-domains.csv` were suggested by Claude and still need the owner's check.

## Commands

Run from the repository root (relative paths in `appsettings.json` resolve from the current folder).

```bash
dotnet build
dotnet test
dotnet run --project src/A11yReport.Scanner -- install-browser                 # one-time Chromium download
dotnet run --project src/A11yReport.Scanner -- url <url>                       # one site: robots.txt + one load
dotnet run --project src/A11yReport.Scanner -- build-list data/crux/tr-202610.csv.gz
dotnet run --project src/A11yReport.Scanner -- scan --run <name> [--limit <n>] # resumable; rerun to resume
dotnet tool run dotnet-ef migrations add <Name> --project src/A11yReport.Storage --startup-project src/A11yReport.Storage
```

Outputs: `output/a11yreport.db` (gitignored), `data/runs/<name>.json` run manifest (committed),
`output/other-tld-candidates.csv` (gitignored, for reviewing the include list).

## Data files

- `data/crux/tr-YYYYMM.csv.gz` + `data/crux/SOURCE.md`: CrUX snapshot and where it came from.
- `data/sites.csv` + `data/sites.source.json`: generated by `build-list`; never edit by hand.
- `data/sectors.csv`: owner-maintained. `build-list` only appends new domains
  (`auto: domain rule` or `auto: needs review`); existing rows are never changed.
- `data/include-domains.csv`: owner-maintained list of Turkish sites on non-Turkish TLDs.
- `data/opt-out.csv`: honored by `build-list` and again at scan time.

## Decisions log

- Solution file uses the new `.slnx` format.
- `coverlet.collector` removed from the test template (coverage not requested).
- **Name:** "Türkiye Web Accessibility Report" (EN) / "Türkiye Web Erişilebilirlik Raporu" (TR).
  Hosting: GitHub Pages. Domain: none yet — the User-Agent methodology link is filled in
  once the Pages URL exists.
- **List size:** 1,000 sites.
- **Sectors:** public sector (**includes municipalities**, `.bel.tr` → public sector),
  banking/finance, e-commerce, universities, news/media, other.
- **Mobile pass:** yes, 390×844. To keep "one page load per site", the mobile pass reuses the
  same page load: after the desktop axe run, the viewport is resized to 390×844 and axe runs
  again. Limitation (documented in the methodology): sites that serve different HTML to mobile
  user agents (for example `m.` sites) are not covered.
- **Per-site lookup:** yes (stage 5). Whether HTML snippets are shown publicly is decided later;
  until then the lookup shows rule ids and counts only.
- **Site list source:** Chrome UX Report (CrUX) country list for Türkiye (`country_tr`),
  monthly snapshot via the public mirror `github.com/zakird/crux-top-lists`
  (data from Google CrUX, CC BY-SA 4.0). Chosen over Tranco because it measures real Chrome
  page loads **in Türkiye**, while Tranco is a global ranking. Record the snapshot month and the
  mirror commit SHA. CrUX ranks are buckets (1K, 10K, …) and list origins, not domains:
  normalize origins to domains and dedupe in stage 2.
- **axe integration:** `Deque.AxeCore.Playwright` NuGet package (accepted by the owner).
  Package version 4.14.x bundles axe-core 4.14.x. axe-core is MPL-2.0: we don't modify it;
  when we distribute anything that contains it (Docker image, stage 8), add a third-party
  notice with a link to the axe-core source.
- **Packages (stage 1):** `Microsoft.Playwright` 1.64.1, `Deque.AxeCore.Playwright` 4.14.0,
  `Microsoft.Extensions.Hosting` 10.0.15 (DI + configuration + options + logging in one
  Microsoft package; pinned to 10.x to match .NET 10).
- **Logging:** messages are defined in `Log.cs` with `[LoggerMessage]` source generation
  (required by analyzer CA1848 and faster). Logs go to stderr, the summary to stdout.
- **Tests:** CA1707 is off only under `tests/` so test names can use `Method_Scenario_Result`.
- **Browser install:** `install-browser` subcommand calls Playwright's own installer, because
  `playwright.ps1` needs PowerShell, which isn't installed on this machine.
- **User-Agent:** `Mozilla/5.0 (compatible; TurkiyeWebA11yReport/0.2; +<MethodologyUrl>)`.
  The robots.txt product token is `TurkiyeWebA11yReport`. `scan` refuses to run while
  `Scan:MethodologyUrl` is empty.
- **Stage 2 architecture change:** new `A11yReport.Storage` project so Core stays free of EF Core.
  `Microsoft.EntityFrameworkCore.Sqlite` 10.0.15, `Microsoft.EntityFrameworkCore.Design` 10.0.15
  (build-time only, `PrivateAssets=all`), `dotnet-ef` 10.0.15 as a local tool (`dotnet-tools.json`).
  Migrations folder is marked `generated_code` for analyzers.
- **Site list rules:** one entry per registrable domain (`.tr` zones from TRABIS/PSL handled);
  `.tr` plus city TLDs `.istanbul` / `.ist` are Turkish; `nc.tr` (Northern Cyprus) excluded;
  best rank bucket first, ties ordered by SHA-256 of the domain (stable, not alphabetical);
  scan URL = the CrUX home origin (`domain` or `www.domain`). Domains that CrUX lists only
  through subdomains (apps, portals) are left out (decided after the pilot, 2026-11-11).
- **Sector rules:** `.gov.tr`, `.bel.tr`, `.pol.tr`, `.mil.tr`, `.tsk.tr` → public;
  `.edu.tr` → university; `.k12.tr` and everything else → needs review.
- **robots.txt (RFC 9309):** fetched with our User-Agent, up to 5 redirects, 500 KiB limit.
  2xx → rules; 401/403/429/503 → Unreachable (blocked, page not loaded); other 4xx → allowed;
  other 5xx → SkippedRobots (treated as disallow); network error → DNS/TLS/Unreachable/Timeout.
- **Rate limit:** `StartGate` (own class; `System.Threading.RateLimiting` is not in the .NET
  base framework) lets one site start every `Run:DelayBetweenSitesSeconds` (3 s) across
  `Run:Concurrency` (3, max 4) parallel browser contexts in one Chromium.
- **Retries:** once, in a second pass, only DnsError/NetworkError, never public sector
  (Timeout removed after the pilot: a timed-out page had often loaded 150–1,000 resources).
- **Duplicates:** a site whose final URL lands on a site already Done in the run → `Duplicate`.
- **Storage limits:** all element counts are kept; selector + snippet stored for at most
  `Storage:MaxNodesPerRule` (20) elements per rule and viewport (data minimization).
- **Audit:** every start/resume is a `RunSession` row (versions, UA, settings JSON). Requests are
  counted per site (robots.txt, main-frame document loads incl. redirects, all other browser
  requests). Known gap: requests of sites interrupted by Ctrl+C are not saved (they are rescanned).
- **CLI change:** single-URL scan is now `-- url <url>` (stage 1 was `-- <url>`).
- **Exclude list:** `data/exclude-domains.csv` (domain, reason) for gambling, adult and piracy
  sites; honored by `build-list` like the opt-out list. All betting sites, licensed ones
  included, are left out of the include list.
- **Pilot fixes (2026-11-11):** (1) a retry replaced the earlier row and lost its request
  counts; `RunStore` now adds the earlier counts. (2) EF Core had created hidden
  `<Type>EntityId` foreign key columns, leaving `RunId`/`SiteScanId`/`ViolationId` at 0;
  relationships are now configured explicitly and migration `FixForeignKeys` copies the old
  links before dropping the hidden columns. Backup of the pre-fix DB: `output/a11yreport.before-fk-fix.db`.
