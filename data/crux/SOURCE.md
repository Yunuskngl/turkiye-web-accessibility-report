# CrUX snapshot source

| File | Source | Mirror commit | Downloaded |
|---|---|---|---|
| `tr-202610.csv.gz` | https://raw.githubusercontent.com/zakird/crux-top-lists/main/data/country/tr/202610.csv.gz | `a67d0c8f09326df09739560bc43205b4f949f8e6` (2026-11-10) | 2026-11-11 |

Data: Chrome UX Report (CrUX) by Google, licensed under CC BY-SA 4.0
(https://developer.chrome.com/docs/crux/). Columns: `origin,rank`, where rank is a bucket
(1000, 5000, 10000, 50000, 100000, 500000) of origins by Chrome page loads in Türkiye.
Origins inside a bucket are not ordered.

## Filtered copy (2026-11-15)

The committed `tr-202610.csv.gz` is a filtered copy: 7 origins of 5 domains that are a person's
own name were removed (personal data). Their SHA-256 domain hashes are in
`data/exclude-hashed.csv`, which keeps them out of future lists too. The 2026 site list was built
from the unfiltered file (sha256 recorded in `data/sites.source.json` and in the published
`report.json`); sha256 of the filtered file: `2bb91d65343cbb872eba7fd504174ec75bf710908210e32505b2d0c49bc033dc`.
