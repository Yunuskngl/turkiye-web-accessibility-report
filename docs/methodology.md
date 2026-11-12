# Methodology

This page describes the 2026 edition of the report.

## In short

- **What we checked:** the home pages of the 1,000 most visited websites in Türkiye, using real
  Chrome visit data.
- **How:** a computer program opened each home page once, like a normal visitor, and ran an
  open-source checking tool (axe-core) on it, on a computer screen and on a phone screen.
- **What we looked for:** problems that make a site hard to use for people with disabilities,
  for example images without a text description, text that is too pale to read, or buttons
  without a name that a screen reader can read out.
- **What it is not:** a legal audit. A program can find only some problems, so "no problems
  found" does not mean a site is easy for everyone to use.
- **Fair play:** we followed each site's robots.txt rules, loaded each home page only once, and
  stopped when a site refused our visit. We do not rank or name sites in the report.
- **Website owners** can ask to be left out (see "Opting out").

The rest of this page gives the technical details.

## What this report is — and is not

This report measures **automatically detectable** accessibility failures on the
**home pages** of popular websites in Türkiye, using the open-source axe-core engine
with the WCAG 2.0, 2.1 and 2.2 Level A and AA rules.

It is **not** a legal compliance audit or a certification. A page with zero detected
failures is **not** necessarily accessible: automated tools find only part of the
accessibility barriers that real users meet. Keyboard use, screen reader experience,
meaningful alt text and many other aspects need human testing.

The public report shows **sector-level aggregates**. It does not rank sites and does not
publish lists of "worst" sites.

## Which sites

- **Source list:** Chrome UX Report (CrUX) popularity data for Türkiye, snapshot of
  October 2026 (`tr-202610`). CrUX ranks origins by real Chrome page loads in a country and
  groups them in rank buckets (top 1,000, 5,000, 10,000, …), not exact ranks.
- **Selection:** domains under `.tr` (and the city domains `.istanbul` and `.ist`), plus
  well-known Turkish sites on other TLDs that we list by hand, up to 1,000 domains.
  `.nc.tr` (Northern Cyprus) is excluded because the report covers Türkiye.
- **One entry per site:** all origins of a registrable domain (for example `www.`, `m.` or a
  student portal subdomain) count as one site, with the best rank bucket among them. We scan
  the home page of the main host (`domain` or `www.domain`). Domains that CrUX lists only
  through subdomains (for example an e-government application or a login portal) are left
  out, because their popularity does not come from a home page.
- **Order and cut-off:** sites are ordered by rank bucket. CrUX does not order sites inside a
  bucket, so the 1,000 cut-off inside the last bucket uses a fixed pseudo-random order
  (SHA-256 of the domain name), which is stable and not alphabetical.
- **Deduplication:** a site whose home page redirects to another site already scanned in the
  same run is counted once (marked "duplicate").
- **Sectors:** public sector (including municipalities), banking/finance, e-commerce,
  universities, news/media, other. Assigned by domain rules (`.gov.tr`, `.bel.tr`, `.pol.tr`,
  `.mil.tr`, `.tsk.tr` → public sector; `.edu.tr` → university) and by manual review of all
  other domains. Unknown sites are "other".

## How each page is scanned

- One page load per site per scan run, plus the resources the browser loads by itself.
- Chromium (via Playwright), desktop viewport 1280×800, locale `tr-TR`.
- Mobile pass: after the desktop check, the same loaded page is resized to 390×844 and checked
  again, so no second page load is needed. Sites that send different pages to mobile browsers
  (for example separate `m.` sites) are therefore not covered by the mobile pass.
- We wait up to 30 seconds for the HTML document to be ready (`DOMContentLoaded`), then up to
  10 more seconds for the page's load event, then 2 seconds more. Many popular pages keep
  loading ads and trackers long after their content is ready; if the load event has not
  happened by then, we check the page anyway and record that it was checked before full load.
- No interaction: no clicks, no scrolling, no cookie banners accepted, no forms submitted.
- axe-core 4.14.0 in Chromium 156 (Playwright 1.64.1). Rule tags: `wcag2a`, `wcag2aa`, `wcag21a`,
  `wcag21aa`, `wcag22aa`.
- Each script step inside the page (reading page facts, one axe run) has a 60-second limit. A page
  that does not answer in time, or that navigates away by itself while it is being checked, is
  recorded as "check could not run".
- Scan dates: see "Full run" below.

## Responsible scanning

- **robots.txt:** fetched before the page, following RFC 9309. The rules for our product
  token `TurkiyeWebA11yReport` apply, otherwise the rules for `*`. If the home page is
  disallowed, or robots.txt answers with a server error, the page is not loaded and the site
  is counted as "skipped (robots.txt)". A missing robots.txt (for example HTTP 404) allows
  scanning, as the standard says.
- **Rate limits:** at most 3 browser tabs in parallel (never more than 4), and at most one new
  site started every 3 seconds across all of them.
- **User-Agent:** `Mozilla/5.0 (compatible; TurkiyeWebA11yReport/<version>; +<link to this page>)`.
- **If a site blocks us, we stop.** HTTP 401, 403, 429 or 503 (on robots.txt or the page) and
  refused or reset connections are recorded as "unreachable", the page is not loaded (or not
  loaded again), and the site is not retried in the same run. We do not use proxies, rotate IP
  addresses, disguise the scanner, or solve CAPTCHAs.
- **Retries:** a site gets at most one retry per run, only after a DNS or other network error
  where no page was received. Pages that time out are not retried, because a slow page has
  often already loaded hundreds of resources. Public-sector sites are never retried.
- **Audit trail:** every run, and every time an interrupted run is resumed, records the date
  and time, tool versions (scanner, .NET, Playwright, browser, axe-core), the site list
  version, the User-Agent and all settings. For every site it records how many requests were
  made: robots.txt requests, page loads (including redirects) and resources the browser loaded
  for the page. The run record is published as a JSON file. Known gap: requests of a site that
  was in progress when a run was stopped by hand are not counted; that site is scanned again
  when the run resumes.

## What we store

Only technical results:

- axe rule id, impact, WCAG tags, number of affected elements
- CSS selector and the HTML snippet reported by axe (cut to 300 characters) for at most 20
  example elements per rule and viewport; the total number of affected elements is always kept
- page facts: final URL after redirects, HTTP status, `lang` attribute value, whether a title
  exists (not the title text), number of landmarks, whether a `main` landmark exists, number of
  headings, number of `h1` headings, and whether a heading level is skipped (for example h2 → h4)

We do **not** store page text, screenshots, cookies or any personal data. Selectors and HTML
snippets stay in our local database and are **not** published; the public lookup shows rule names
and counts only.

## How the numbers are counted

- A **detected failure** is one page element that fails one axe rule. A page with 12 images
  without alternative text has 12 detected failures of that rule.
- Share of home pages with at least one detected failure, and the average number of detected
  failures per page (mean), for the desktop and the mobile view.
- Sector comparisons and the list of most common failure types use the desktop view. A failure
  type's share is the share of checked home pages with at least one such failure.
- `lang` usage (missing, or set to Turkish such as `tr` or `tr-TR`), whether a `main` landmark
  exists, the number of landmarks and headings, pages with exactly one or no `h1`, and pages
  that skip a heading level.
- Sites that were skipped, unreachable or failed are reported separately and are **never**
  counted as having zero failures. Opted-out sites are left out of all published numbers.
- The public report shows sector-level numbers only, never a list or ranking of sites.

## How we differ from the WebAIM Million

WebAIM uses the WAVE engine, we use axe-core; the two engines check partly different rules (see
"Manual comparison with WAVE"), so our counts are not directly comparable with WebAIM's. Our
list is limited to Türkiye and comes from CrUX; we report by sector; we also check a mobile
viewport.

## Full run (November 2026)

The full run (`2026-full`, run record in `data/runs/2026-full.json`) checked the 1,000 sites of
the list on 11 November 2026 between 14:59 and 18:18 Türkiye time (UTC+3), with axe-core 4.14.0, Chromium
156 and Playwright 1.64.1.

| Result | Sites |
|---|---|
| Checked | 878 |
| Refused the request or unreachable (57 × HTTP 403, 1 × 429, 1 × 503, 1 refused connection) | 60 |
| Did not load in time | 33 |
| Secure connection (TLS) failed | 11 |
| Skipped: robots.txt does not allow it | 5 |
| Check could not run | 5 |
| Network error, HTTP error, domain not found (2 each) | 6 |
| Redirects to another listed site | 2 |

Requests: 139,732 in total (1,028 robots.txt requests, 1,026 page loads, 137,678 resources loaded
by the browser for those pages).

Notes on this run:

- **The run was interrupted once.** At site 586 a page navigated away by itself while our
  scanner was reading it, and an unhandled error stopped the scanner. We added a time limit to
  every check step inside the page and record such pages as "check could not run", then resumed
  the run; the 586 finished sites were not scanned again. Both sessions and their settings are in
  the run record. The requests of the few sites that were in progress when the scanner stopped
  are not counted; those sites were scanned again on resume.
- **44.1% of the checked pages** had not finished loading extra content (the browser's load
  event) 10 seconds after the page was ready. They were checked anyway; see "How each page is
  scanned".
- **Averages and outliers:** the average is the arithmetic mean. The number of detected failures
  per page varies a lot: half of the checked home pages had 29 or fewer (median), 90% had 113 or
  fewer, and one page had 5,911. 40 pages (4.6%) had none.
- **Possible placeholder pages:** 7 checked pages loaded fewer than five resources. Some of them
  may have shown a bot check or maintenance page instead of the real home page. We could not
  tell this reliably, so they are counted as checked.

## Pilot (November 2026)

A pilot run (`2026-pilot`, run record in `data/runs/2026-pilot.json`) scanned the first 50
sites of the list on 2026-11-11 with axe-core 4.14.0, Chromium 156 and Playwright 1.64.1.

| Result | Sites |
|---|---|
| Scanned | 40 |
| Did not finish loading in 30 seconds | 6 |
| Refused the request (HTTP 403) | 2 |
| Domain name not found | 2 |

Requests: 9,808 in total (50 robots.txt requests, 51 page loads, 9,707 resources loaded by the
browser for those pages).

Findings that changed or may change the method:

- Two scanner defects found in the pilot were fixed before any results were used: retried
  sites lost the request counts of their first attempt, and stored rule results were not linked
  to their site in the database (the data itself was intact and was relinked). Because of the
  first defect, the pilot run record undercounts the requests of the 9 retried sites
  (one extra page load each, except the DNS failure).
- 6 of 50 pages did not fire the browser's load event within 30 seconds, while their content
  was ready (they loaded 150–1,000 resources). We now wait for the document to be ready and give
  the load event 10 more seconds (see "How each page is scanned"), and timeouts are no longer
  retried.
- 2 of 50 sites failed because CrUX listed only an application subdomain (for example
  `uygulama.gtb.gov.tr`) and the guessed `www.` home page did not exist. Such domains are now
  left out of the list; this replaced 42 of the 1,000 sites, including some universities and
  public bodies whose subdomains are more visited than their home page.

### Manual comparison with WAVE

On 2026-11-11 the project owner checked five pilot sites by hand with the WAVE browser
extension (desktop browser window, cookie banner left untouched, as in our scan). Our numbers
are from the pilot scan (desktop viewport), taken about one hour earlier.

| Site | WAVE errors | WAVE contrast errors | Our failed rules / elements | Our main findings |
|---|---|---|---|---|
| milliyet.com.tr | 26 (form label 16, empty link 8, alt text 2) | 92 | 6 / 50 | contrast 31, link name 12, alt text 3 |
| migros.com.tr | 1 (form label 1) | 83 | 5 / 83 | contrast 76, label/name mismatch 4 |
| yapikredi.com.tr | 11 (form label 6, alt text 2, empty link 2, empty button 1) | 0 | 7 / 23 | list structure 12, ARIA roles 7, form label 2 |
| metu.edu.tr | 3 (empty link 3) | 1 | 3 / 13 | target size 7, ARIA attribute 5, frame title 1 |
| saglik.gov.tr | 0 | 2 | 0 / 0 | none |

What we learned:

- **Contrast results agree in four of five sites** (83 vs 76, 0 vs 0, 1 vs 0, 2 vs 0). The
  exception is a news home page (92 vs 31) whose content changed between the two checks.
- **The two tools check different things.** axe reports list structure, required ARIA roles,
  target size (WCAG 2.2), label/name mismatch and zoom-blocking `meta viewport` as failures.
  WAVE does not check some of these and reports others as alerts, not errors.
- **WAVE reported some empty links, empty buttons, missing alt text and form labels that axe
  did not.** A likely reason is that WAVE also evaluates some hidden content (menus, dialogs,
  cookie settings), while axe checks only content that is rendered for users. We have not
  verified this element by element.
- **A result of zero is real, not a blocked page:** saglik.gov.tr had no failures in our scan
  and almost none in WAVE.
- **Consequence for this report:** our numbers come from axe-core and are not directly
  comparable with the WebAIM Million, which uses the WAVE engine. We compare sectors with each
  other and do not compare our averages with WebAIM's.

## Looking up a site

The [site lookup page](lookup.html) shows the result of one home page when someone types its
address: which automated rules failed, how many elements were affected, and the page's language
and heading facts. It shows rule names and counts only, not the HTML of the page. There is no
list or ranking of sites: each result is stored in a file named after the SHA-256 hash of the
domain, so a result can only be found by typing that domain. The list of scanned domains is still
public in the source repository, for reproducibility.

## Turkish explanations of failure types

The site lookup can show a short Turkish explanation and a fix example for each failure type.

- A language model running on our own computer (Ollama with the open `gemma3:4b` model) wrote a
  first draft for each axe rule that failed at least once.
- The model received **only axe-core's own description of the rule** (rule id, summary,
  description and WCAG criteria). It never received anything from the scanned websites.
- The small local model's drafts were often technically wrong (for example wrong "before" code,
  identical "before" and "after" examples, or the wrong group of affected users). All 27 drafts
  were therefore rewritten by Claude (an AI assistant) from axe-core's rule definitions, and each
  entry records this in its notes.
- The project owner checks every text before it is published. Only explanations marked as
  approved are published; all versions are kept in `data/explanations/rules.tr.json`.

## Opting out

Site owners can ask us not to scan their site and not to publish its result.

- **How:** open an
  [opt-out request](https://github.com/Yunuskngl/turkiye-web-accessibility-report/issues/new?template=opt-out.yml)
  in the project's GitHub repository. Only the domain is needed. GitHub issues are public, so
  please do not add personal data.
- **No proof needed:** leaving a site out does little harm, so we honor every request without
  asking for proof of ownership.
- **What happens:** the domain is added to `data/opt-out.csv`. From the next run it is not
  scanned, and from the next publication of this site it has no lookup result and is not counted
  in any published number. Requests are handled by hand, so the change appears at the next
  publication.

## Data sources and licenses

- Site list: Chrome UX Report (CrUX) by Google, licensed under CC BY-SA 4.0
  (https://developer.chrome.com/docs/crux/), via the snapshots at
  https://github.com/zakird/crux-top-lists (commit `a67d0c8f09326df09739560bc43205b4f949f8e6`).
- axe-core by Deque Systems, licensed under MPL-2.0: https://github.com/dequelabs/axe-core
