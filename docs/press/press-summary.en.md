# Press summary: Türkiye Web Accessibility Report 2026

**Automatically detectable accessibility failures were found on 95% of the home pages of
Türkiye's most visited websites.**

The Türkiye Web Accessibility Report checked the home pages of the 1,000 most visited websites in
Türkiye automatically on 11 November 2026. 878 home pages could be checked.

## Key findings

- **95.4%** of the checked home pages had at least one detected accessibility failure (desktop
  view; 95.1% in the mobile view).
- On average, **58.8** failures were detected per home page (55.3 on mobile). Half of the pages had
  29 or fewer; very high numbers on a few pages raise the average.
- **Most common problems:** low contrast text (75.7% of pages), links without a text name (64.6%),
  images without alternative text (45.3%) and touch targets that are too small (44.9%).
- **26.7%** of the pages block zooming on mobile devices.
- **90.9%** of the pages set Turkish as the page language; **45.3%** have no first-level
  heading (h1).

## By sector (average detected failures per home page, desktop)

| Sector | Pages checked | Average failures |
|---|---|---|
| Public sector | 92 | 29.7 |
| Banking and finance | 50 | 34.4 |
| Universities | 51 | 41.6 |
| News and media | 167 | 61.2 |
| E-commerce | 234 | 67.0 |
| Other | 284 | 67.6 |

## Important limits

- This is an **automated** check, not a legal compliance audit. Automated tools find only part of
  accessibility barriers; no detected failures does not mean a site is accessible.
- Only home pages were checked. The report does not rank or name sites; results are sector-level.
- Results come from axe-core and are not directly comparable with reports that use other tools,
  such as the WebAIM Million.
- 122 sites could not be checked (60 refused the request, 33 did not load in time, the rest had
  technical errors); they are not counted as having zero failures.

## Method in brief

Site list: Chrome UX Report (CrUX) data for Türkiye, October 2026. Tool: axe-core 4.14.0, WCAG 2.2 A
and AA rules, desktop (1280×800) and mobile (390×844) view. One page load per site, robots.txt
respected, and sites that blocked us were not retried.

## Links

- Report: https://yunuskngl.github.io/turkiye-web-accessibility-report/
- Methodology: https://yunuskngl.github.io/turkiye-web-accessibility-report/methodology.html
- Source code and data: https://github.com/Yunuskngl/turkiye-web-accessibility-report
