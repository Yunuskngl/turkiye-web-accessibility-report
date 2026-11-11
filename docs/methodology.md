# Methodology (draft)

> Draft written in stage 0. Values marked _TBD_ are filled in after the pilot (stage 3)
> and the full run (stage 7).

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

- **Source list:** Chrome UX Report (CrUX) popularity data for Türkiye. Draft list: snapshot
  October 2026 (`tr-202610`). CrUX ranks origins by real Chrome page loads in a country and
  groups them in rank buckets (top 1,000, 5,000, 10,000, …), not exact ranks.
- **Selection:** domains under `.tr` (and the city domains `.istanbul` and `.ist`), plus
  well-known Turkish sites on other TLDs that we list by hand, up to 1,000 domains.
  `.nc.tr` (Northern Cyprus) is excluded because the report covers Türkiye.
- **One entry per site:** all origins of a registrable domain (for example `www.`, `m.` or a
  student portal subdomain) count as one site, with the best rank bucket among them. We scan
  the home page of the main host (`domain` or `www.domain`).
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
- 30-second navigation timeout, then a short settle time after the page's load event.
- No interaction: no clicks, no scrolling, no cookie banners accepted, no forms submitted.
- axe-core version: _TBD_. Rule tags: `wcag2a`, `wcag2aa`, `wcag21a`, `wcag21aa`, `wcag22aa`.
- Scan date(s): _TBD_.

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
- **Retries:** a site gets at most one retry per run, only after a timeout, DNS or other
  network error where no page was received. Public-sector sites are never retried.
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

We do **not** store page text, screenshots, cookies or any personal data.
_Open point: whether HTML snippets are shown in the public per-site lookup or kept internal only._

## How the numbers are counted

- Share of home pages with at least one detected failure.
- Average number of detected failures per page, overall and per sector.
- Most common failure types (by axe rule).
- `lang` usage (including `lang="tr"`), landmark usage, heading structure.
- Sites that were skipped, unreachable or failed are reported separately and are **never**
  counted as having zero failures.

## How we differ from the WebAIM Million

_TBD after stage 3/4._ Known differences so far: WebAIM uses the WAVE engine, we use axe-core;
our list is limited to Türkiye; we report by sector.

## Manual comparison (pilot)

_TBD in stage 3:_ five sites checked manually with the WAVE browser extension and compared
with our results.

## Opting out

Site owners can ask not to be scanned or listed. _TBD: contact channel (stage 5)._
Opted-out domains are excluded from scanning and from all published data.

## Data sources and licenses

- Site list: Chrome UX Report (CrUX) by Google, licensed under CC BY-SA 4.0
  (https://developer.chrome.com/docs/crux/), via the snapshots at
  https://github.com/zakird/crux-top-lists (commit _TBD_).
- axe-core by Deque Systems, licensed under MPL-2.0: https://github.com/dequelabs/axe-core
