// Site lookup: normalizes the typed address to the same domain key the scanner uses (DomainKey.FromHost),
// hashes it with SHA-256 and loads sites/<hash>.json. All page text from data is set with textContent.
(function () {
  'use strict';

  const form = document.getElementById('lookup-form');
  const input = document.getElementById('domain');
  const status = document.getElementById('lookup-status');
  const result = document.getElementById('lookup-result');
  const trZones = new Set((form.dataset.trZones || '').split(' '));
  let explanations = null;

  // Approved Turkish explanations, loaded once. A missing file just means none are published yet.
  function loadExplanations() {
    explanations = explanations || fetch('explanations.tr.json')
      .then((response) => (response.ok ? response.json() : []))
      .catch(() => []);
    return explanations;
  }

  function domainKey(text) {
    let host = text.trim().toLowerCase().replace(/^[a-z][a-z0-9+.-]*:\/\//, '');
    host = host.split(/[/?#]/)[0].split(':')[0].replace(/\.$/, '');
    if (host.startsWith('www.')) {
      host = host.slice(4);
    }
    const labels = host.split('.');
    if (labels.length <= 2) {
      return host;
    }
    const isTrZone = labels[labels.length - 1] === 'tr' && trZones.has(labels[labels.length - 2]);
    return labels.slice(isTrZone ? -3 : -2).join('.');
  }

  async function sha256Hex(text) {
    const bytes = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(text));
    return Array.from(new Uint8Array(bytes), (b) => b.toString(16).padStart(2, '0')).join('');
  }

  function element(tag, text, className) {
    const node = document.createElement(tag);
    if (text !== undefined) {
      node.textContent = String(text);
    }
    if (className) {
      node.className = className;
    }
    return node;
  }

  function table(caption, headers, rows) {
    const wrap = element('div', undefined, 'table-wrap');
    wrap.setAttribute('role', 'region');
    wrap.setAttribute('tabindex', '0');
    wrap.setAttribute('aria-label', caption);
    const tableNode = element('table');
    tableNode.appendChild(element('caption', caption));
    const headRow = element('tr');
    headers.forEach((h) => {
      const th = element('th', h);
      th.scope = 'col';
      headRow.appendChild(th);
    });
    tableNode.appendChild(element('thead')).appendChild(headRow);
    const body = tableNode.appendChild(element('tbody'));
    rows.forEach((cells) => {
      const row = body.appendChild(element('tr'));
      cells.forEach((cell, i) => {
        const td = element(i === 0 ? 'th' : 'td', cell);
        if (i === 0) {
          td.scope = 'row';
        }
        row.appendChild(td);
      });
    });
    wrap.appendChild(tableNode);
    return wrap;
  }

  function viewportSection(viewport) {
    const section = element('section');
    section.appendChild(element('h3', viewport.name + ' view'));
    if (viewport.rules.length === 0) {
      section.appendChild(element('p', 'No failures detected by the automated rules. This does not mean the page is accessible.'));
      return section;
    }
    const rows = viewport.rules.map((r) => [r.name + ' (' + r.ruleId + ')', r.criteria || '–', r.elements]);
    section.appendChild(table(viewport.name + ' view: detected failures', ['Failure type (axe rule)', 'WCAG criteria', 'Elements'], rows));
    return section;
  }

  function factsTable(facts) {
    const yesNo = (value) => (value ? 'Yes' : 'No');
    return table('Page language and structure', ['Measure', 'Value'], [
      ['Page language (lang attribute)', facts.lang ? facts.lang : 'Not set'],
      ['Has a page title', yesNo(facts.hasTitle)],
      ['Has a main landmark', yesNo(facts.hasMain)],
      ['First-level headings (h1)', facts.h1Count],
      ['Headings in total', facts.headingCount],
      ['Skips a heading level', yesNo(facts.skipsHeadingLevel)],
    ]);
  }

  function explanationSection(site, list) {
    const ruleIds = new Set(site.viewports.flatMap((v) => v.rules.map((r) => r.ruleId)));
    const items = list.filter((e) => ruleIds.has(e.ruleId));
    if (items.length === 0) {
      return null;
    }
    const section = element('section');
    section.lang = 'tr';
    section.appendChild(element('h3', 'Bu hatalar ne anlama geliyor?'));
    section.appendChild(element('p', 'Bu açıklamalar yapay zekâ ile taslak olarak yazıldı ve yayımlanmadan önce proje sahibi tarafından incelendi.'));
    items.forEach((e) => {
      section.appendChild(element('h4', e.title + ' (' + e.ruleId + ')'));
      section.appendChild(element('p', e.explanation));
      const pre = element('pre', undefined, 'fix-example');
      pre.appendChild(element('code', e.fixExample));
      section.appendChild(pre);
    });
    return section;
  }

  function render(site, list) {
    result.replaceChildren();
    const heading = element('h2', 'Result for ' + site.domain);
    heading.tabIndex = -1;
    result.appendChild(heading);
    result.appendChild(element('p', 'Sector: ' + site.sectorName + '. Checked on ' + site.checkedOn + ' with axe-core ' + site.axeVersion + '.'));
    if (!site.checked) {
      result.appendChild(element('p', 'This home page was not checked: ' + site.statusText + '. It is not counted as a page without failures.'));
    } else {
      const summary = site.viewports.map((v) => [v.name + ' view', v.failedRules, v.elements]);
      result.appendChild(table('Summary', ['View', 'Failed rules', 'Detected failures (elements)'], summary));
      site.viewports.forEach((v) => result.appendChild(viewportSection(v)));
      if (site.facts) {
        result.appendChild(factsTable(site.facts));
      }
      if (site.loadEventReached === false) {
        result.appendChild(element('p', 'The page was still loading extra content (for example ads) when it was checked.'));
      }
      const explained = explanationSection(site, list);
      if (explained) {
        result.appendChild(explained);
      }
    }
    heading.focus();
  }

  async function lookUp(event) {
    event.preventDefault();
    result.replaceChildren();
    const key = domainKey(input.value);
    if (!key || !key.includes('.')) {
      status.textContent = 'Please enter a website address, for example example.com.tr.';
      input.focus();
      return;
    }
    status.textContent = 'Looking up ' + key + '…';
    try {
      const response = await fetch('sites/' + (await sha256Hex(key)) + '.json');
      if (!response.ok) {
        status.textContent = 'No result for ' + key + '. The site was not in this run, or its owner asked to be left out.';
        return;
      }
      const site = await response.json();
      const list = await loadExplanations();
      status.textContent = '';
      render(site, list);
    } catch (error) {
      status.textContent = 'The lookup could not be completed. Please try again.';
    }
  }

  form.addEventListener('submit', lookUp);
}());
