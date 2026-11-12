// Site lookup: normalizes the typed address to the same domain key the scanner uses (DomainKey.FromHost),
// hashes it with SHA-256 and loads sites/<hash>.json. All page text from data is set with textContent.
// The page language comes from the form (data-lang); its texts come from the JSON in #lookup-texts.
(function () {
  'use strict';

  const form = document.getElementById('lookup-form');
  const input = document.getElementById('domain');
  const status = document.getElementById('lookup-status');
  const result = document.getElementById('lookup-result');
  const trZones = new Set((form.dataset.trZones || '').split(' '));
  const root = form.dataset.root || '';
  const lang = form.dataset.lang || 'en';
  const isTurkish = lang === 'tr';
  const t = JSON.parse(document.getElementById('lookup-texts').textContent);
  let explanations = null;

  function text(key, values) {
    return Object.keys(values || {}).reduce((s, k) => s.split('{' + k + '}').join(values[k]), t[key] || key);
  }

  // Approved Turkish explanations, loaded once. A missing file just means none are published yet.
  function loadExplanations() {
    explanations = explanations || fetch(root + 'explanations.tr.json')
      .then((response) => (response.ok ? response.json() : []))
      .catch(() => []);
    return explanations;
  }

  function domainKey(value) {
    let host = value.trim().toLowerCase().replace(/^[a-z][a-z0-9+.-]*:\/\//, '');
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

  async function sha256Hex(value) {
    const bytes = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(value));
    return Array.from(new Uint8Array(bytes), (b) => b.toString(16).padStart(2, '0')).join('');
  }

  function formatDate(isoDate) {
    const date = new Date(isoDate + 'T00:00:00Z');
    return new Intl.DateTimeFormat(isTurkish ? 'tr-TR' : 'en-GB', { day: 'numeric', month: 'long', year: 'numeric', timeZone: 'UTC' }).format(date);
  }

  function element(tag, content, className) {
    const node = document.createElement(tag);
    if (content !== undefined) {
      node.textContent = String(content);
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
    const viewName = text(viewport.name);
    section.appendChild(element('h3', viewName));
    if (viewport.rules.length === 0) {
      section.appendChild(element('p', text('noFailures')));
      return section;
    }
    const rows = viewport.rules.map((r) => [(isTurkish ? r.nameTr : r.name) + ' (' + r.ruleId + ')', r.criteria || '–', r.elements]);
    section.appendChild(table(text('rulesCaption', { view: viewName }),
      [text('ruleColumn'), text('criteriaColumn'), text('elementsColumn')], rows));
    return section;
  }

  function factsTable(facts) {
    const yesNo = (value) => (value ? text('yes') : text('no'));
    return table(text('factsCaption'), [text('measure'), text('value')], [
      [text('lang'), facts.lang ? facts.lang : text('notSet')],
      [text('hasTitle'), yesNo(facts.hasTitle)],
      [text('hasMain'), yesNo(facts.hasMain)],
      [text('h1Count'), facts.h1Count],
      [text('headingCount'), facts.headingCount],
      [text('skips'), yesNo(facts.skipsHeadingLevel)],
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
    section.appendChild(element('h3', 'Bu sorunlar ne anlama geliyor?'));
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
    const heading = element('h2', text('resultFor', { domain: site.domain }));
    heading.tabIndex = -1;
    result.appendChild(heading);
    result.appendChild(element('p', text('sectorLine', {
      sector: isTurkish ? site.sectorNameTr : site.sectorName, date: formatDate(site.checkedOn), axe: site.axeVersion,
    })));
    if (!site.checked) {
      result.appendChild(element('p', text('notChecked', { status: isTurkish ? site.statusTextTr : site.statusText })));
    } else {
      const summary = site.viewports.map((v) => [text(v.name), v.failedRules, v.elements]);
      result.appendChild(table(text('summary'), [text('view'), text('failedRules'), text('detectedFailures')], summary));
      site.viewports.forEach((v) => result.appendChild(viewportSection(v)));
      if (site.facts) {
        result.appendChild(factsTable(site.facts));
      }
      if (site.loadEventReached === false) {
        result.appendChild(element('p', text('stillLoading')));
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
      status.textContent = text('enterAddress');
      input.focus();
      return;
    }
    status.textContent = text('lookingUp', { domain: key });
    try {
      const response = await fetch(root + 'sites/' + (await sha256Hex(key)) + '.json');
      if (!response.ok) {
        status.textContent = text('noResult', { domain: key });
        return;
      }
      const site = await response.json();
      const list = await loadExplanations();
      status.textContent = '';
      render(site, list);
    } catch (error) {
      status.textContent = text('failed');
    }
  }

  form.addEventListener('submit', lookUp);
}());
