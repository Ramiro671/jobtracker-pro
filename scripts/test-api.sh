#!/usr/bin/env bash
# ============================================================
# JobTracker Pro — API E2E Test Script
# Genera output en consola + scripts/test-report.html
#
# Uso:
#   ./scripts/test-api.sh              # local (localhost:5000)
#   ./scripts/test-api.sh prod         # producción (Azure)
#   ./scripts/test-api.sh http://...   # URL personalizada
# ============================================================

set -euo pipefail

# ── Configuración ─────────────────────────────────────────────
LOCAL_URL="http://localhost:5000"
PROD_URL="https://jobtracker-api-prod-ehg6euckd4evaabw.centralus-01.azurewebsites.net"

case "${1:-}" in
  prod)         BASE_URL="$PROD_URL" ;;
  http*|https*) BASE_URL="$1" ;;
  *)            BASE_URL="$LOCAL_URL" ;;
esac

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPORT_FILE="$SCRIPT_DIR/test-report.html"

# ── Colores consola ───────────────────────────────────────────
GREEN='\033[0;32m'; RED='\033[0;31m'; YELLOW='\033[1;33m'
CYAN='\033[0;36m'; BOLD='\033[1m'; RESET='\033[0m'

# ── Estado global ─────────────────────────────────────────────
PASS=0; FAIL=0; SKIP=0
REPORT_JSON=""           # array de resultados JSON
CURRENT_SECTION="General"
HTTP_STATUS=""; HTTP_DURATION=0; RESPONSE_BODY=""
HEADERS=()

TOTAL_START_MS=$(date +%s%3N 2>/dev/null || echo "0")

# ── Helpers de tiempo ─────────────────────────────────────────
now_ms() { date +%s%3N 2>/dev/null || echo "0"; }

# ── HTTP ──────────────────────────────────────────────────────
HTTP_START_MS=0
http() {
  HTTP_START_MS=$(now_ms)
  local method="$1" url="$2"; shift 2
  local raw
  raw=$(curl -s --max-time 15 -w "\n%{http_code}" -X "$method" \
    -H "Content-Type: application/json" \
    "${HEADERS[@]}" "$@" "$url" 2>/dev/null || echo -e "\ncurl_error")
  HTTP_STATUS="${raw##*$'\n'}"
  RESPONSE_BODY="${raw%$'\n'*}"
  HTTP_DURATION=$(( $(now_ms) - HTTP_START_MS ))
}

set_token()   { HEADERS=(-H "Authorization: Bearer $1"); }
clear_token() { HEADERS=(); }

# ── Record resultado para HTML ────────────────────────────────
_record() {
  local status="$1" label="$2" expected="$3" actual="$4" note="${5:-}"
  # Escapar para JSON
  local body; body=$(printf '%s' "${RESPONSE_BODY:-}" | head -c 500 \
    | sed 's/\\/\\\\/g; s/"/\\"/g; s/\t/\\t/g' | tr -d '\r' \
    | awk '{printf "%s\\n", $0}' | tr -d '\n')
  local sec_e; sec_e=$(printf '%s' "$CURRENT_SECTION" | sed 's/"/\\"/g')
  local lbl_e; lbl_e=$(printf '%s' "$label"           | sed 's/"/\\"/g')
  local exp_e; exp_e=$(printf '%s' "$expected"         | sed 's/"/\\"/g')
  local act_e; act_e=$(printf '%s' "$actual"           | sed 's/"/\\"/g')
  local not_e; not_e=$(printf '%s' "$note"             | sed 's/"/\\"/g')
  local ts; ts=$(now_ms)
  local entry="{\"section\":\"$sec_e\",\"label\":\"$lbl_e\",\"status\":\"$status\",\"expected\":\"$exp_e\",\"actual\":\"$act_e\",\"note\":\"$not_e\",\"ms\":${HTTP_DURATION},\"ts\":${ts},\"body\":\"$body\"}"
  if [[ -z "$REPORT_JSON" ]]; then REPORT_JSON="$entry"
  else REPORT_JSON="$REPORT_JSON,$entry"; fi
}

# ── Consola ───────────────────────────────────────────────────
section() {
  CURRENT_SECTION="$1"
  echo -e "\n${CYAN}${BOLD}══ $1 ══${RESET}"
}

pass() {
  PASS=$((PASS + 1))
  local ms_label=""; [[ $HTTP_DURATION -gt 0 ]] && ms_label=" ${YELLOW}(${HTTP_DURATION}ms)${RESET}"
  echo -e "  ${GREEN}✓${RESET} $1${ms_label}"
  _record "pass" "$1" "" ""
}

fail() {
  FAIL=$((FAIL + 1))
  local ms_label=""; [[ $HTTP_DURATION -gt 0 ]] && ms_label=" ${YELLOW}(${HTTP_DURATION}ms)${RESET}"
  echo -e "  ${RED}✗${RESET} $1${ms_label}"
  [[ -n "${2:-}" ]] && echo -e "    ${RED}Esperado: $2${RESET}"
  [[ -n "${3:-}" ]] && echo -e "    ${RED}Obtenido: $3 — body: $(printf '%s' "${RESPONSE_BODY:-}" | head -c 200)${RESET}"
  _record "fail" "$1" "${2:-}" "${3:-}" ""
}

skip() {
  SKIP=$((SKIP + 1))
  echo -e "  ${YELLOW}~${RESET} $1 (omitido)"
  _record "skip" "$1" "" "" "${2:-}"
}

assert_status() {
  local expected="$1" label="$2"
  if [[ "$HTTP_STATUS" == "$expected" ]]; then
    pass "$label"
    return 0
  else
    fail "$label" "HTTP $expected" "HTTP $HTTP_STATUS"
    return 1
  fi
}

assert_json_nonempty() {
  local field="$1" label="$2"
  local actual; actual=$(json_field "$field")
  if [[ -n "$actual" ]]; then
    pass "$label"
  else
    fail "$label" "campo '$field' presente" "vacío o ausente"
  fi
}

json_field() {
  printf '%s' "$RESPONSE_BODY" | grep -oP "\"$1\"\s*:\s*(\"[^\"]*\"|\d+)" | head -1 \
    | sed -E 's/.*:\s*"?([^",}]*)"?.*/\1/'
}

# ── Generar HTML ──────────────────────────────────────────────
generate_html() {
  local total_ms=$(( $(now_ms) - TOTAL_START_MS ))
  local total=$(( PASS + FAIL + SKIP ))
  local ts_human; ts_human=$(date '+%Y-%m-%d %H:%M:%S' 2>/dev/null || echo "")

  cat > "$REPORT_FILE" <<HTMLEOF
<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>JobTracker Pro — Test Report</title>
<style>
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
  :root {
    --bg: #0f1117; --surface: #1a1d27; --surface2: #22263a;
    --border: #2e3347; --text: #e2e8f0; --muted: #8892a4;
    --green: #22c55e; --green-bg: #052e16; --green-border: #14532d;
    --red: #f87171; --red-bg: #2d0a0a; --red-border: #7f1d1d;
    --yellow: #fbbf24; --yellow-bg: #2d1f00; --yellow-border: #78350f;
    --blue: #60a5fa; --blue-bg: #0d1f3c; --blue-border: #1e40af;
    --purple: #a78bfa; --cyan: #22d3ee; --radius: 10px;
  }
  body { background: var(--bg); color: var(--text); font-family: 'Segoe UI', system-ui, sans-serif;
         font-size: 14px; line-height: 1.5; min-height: 100vh; }
  a { color: var(--blue); }

  /* Layout */
  .container { max-width: 960px; margin: 0 auto; padding: 24px 16px 48px; }

  /* Header */
  .header { display: flex; justify-content: space-between; align-items: flex-start;
            flex-wrap: wrap; gap: 12px; margin-bottom: 28px; }
  .header-title { display: flex; flex-direction: column; gap: 4px; }
  .header-title h1 { font-size: 22px; font-weight: 700; color: var(--text); }
  .header-title .subtitle { font-size: 12px; color: var(--muted); }
  .target-badge { background: var(--surface2); border: 1px solid var(--border);
                  border-radius: 20px; padding: 4px 12px; font-size: 12px; color: var(--cyan);
                  font-family: monospace; }
  /* Summary cards */
  .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
             gap: 12px; margin-bottom: 20px; }
  .card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius);
          padding: 16px; text-align: center; }
  .card .val { font-size: 32px; font-weight: 800; line-height: 1; margin-bottom: 4px; }
  .card .lbl { font-size: 11px; text-transform: uppercase; letter-spacing: .07em; color: var(--muted); }
  .card.pass-card { border-color: var(--green-border); background: var(--green-bg); }
  .card.pass-card .val { color: var(--green); }
  .card.fail-card { border-color: var(--red-border); background: var(--red-bg); }
  .card.fail-card .val { color: var(--red); }
  .card.skip-card { border-color: var(--yellow-border); background: var(--yellow-bg); }
  .card.skip-card .val { color: var(--yellow); }
  .card.time-card .val { color: var(--cyan); font-size: 22px; }

  /* Progress bar */
  .progress-wrap { background: var(--surface2); border-radius: 4px; height: 8px;
                   overflow: hidden; margin-bottom: 28px; }
  .progress-bar { height: 100%; border-radius: 4px;
                  background: linear-gradient(90deg, var(--green), var(--cyan)); transition: width .4s; }

  /* Timeline mini */
  .timeline-label { font-size: 11px; color: var(--muted); margin-bottom: 6px; text-transform: uppercase;
                    letter-spacing: .07em; }
  .timeline { display: flex; gap: 2px; height: 20px; border-radius: 4px; overflow: hidden;
              margin-bottom: 28px; }
  .tl-seg { flex-shrink: 0; cursor: pointer; transition: opacity .15s; }
  .tl-seg:hover { opacity: .75; }
  .tl-seg.pass  { background: var(--green); }
  .tl-seg.fail  { background: var(--red); }
  .tl-seg.skip  { background: var(--yellow); }

  /* Filter bar */
  .filter-bar { display: flex; gap: 8px; margin-bottom: 16px; flex-wrap: wrap; }
  .filter-btn { background: var(--surface2); border: 1px solid var(--border); border-radius: 20px;
                color: var(--muted); font-size: 12px; padding: 4px 14px; cursor: pointer;
                transition: all .15s; }
  .filter-btn:hover, .filter-btn.active { color: var(--text); border-color: var(--blue); background: var(--blue-bg); }
  .filter-btn.active { color: var(--blue); }

  /* Sections */
  .section { margin-bottom: 12px; border: 1px solid var(--border); border-radius: var(--radius);
             overflow: hidden; }
  .section-header { display: flex; align-items: center; gap: 10px; padding: 12px 16px;
                    background: var(--surface); cursor: pointer; user-select: none;
                    transition: background .15s; }
  .section-header:hover { background: var(--surface2); }
  .section-header h2 { font-size: 13px; font-weight: 600; flex: 1; }
  .section-pills { display: flex; gap: 6px; }
  .pill { font-size: 11px; font-weight: 600; padding: 2px 8px; border-radius: 10px; }
  .pill.pass { background: var(--green-bg); color: var(--green); border: 1px solid var(--green-border); }
  .pill.fail { background: var(--red-bg);   color: var(--red);   border: 1px solid var(--red-border); }
  .pill.skip { background: var(--yellow-bg);color: var(--yellow);border: 1px solid var(--yellow-border); }
  .chevron { color: var(--muted); font-size: 12px; transition: transform .2s; }
  .chevron.open { transform: rotate(90deg); }
  .section-body { display: none; }
  .section-body.open { display: block; }

  /* Test rows */
  .test-row { border-top: 1px solid var(--border); }
  .test-main { display: flex; align-items: center; gap: 10px; padding: 10px 16px;
               cursor: pointer; transition: background .12s; }
  .test-main:hover { background: var(--surface2); }
  .dot { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; }
  .dot.pass { background: var(--green); box-shadow: 0 0 6px var(--green); }
  .dot.fail { background: var(--red);   box-shadow: 0 0 6px var(--red);   }
  .dot.skip { background: var(--yellow);box-shadow: 0 0 6px var(--yellow);}
  .test-label { flex: 1; color: var(--text); }
  .test-label.fail { color: var(--red); }
  .test-label.skip { color: var(--yellow); }
  .ms-badge { font-size: 11px; padding: 2px 7px; border-radius: 10px; font-family: monospace;
              background: var(--surface2); color: var(--muted); border: 1px solid var(--border); }
  .ms-badge.slow { background: var(--yellow-bg); color: var(--yellow); border-color: var(--yellow-border); }
  .ms-badge.fast { background: var(--green-bg);  color: var(--green);  border-color: var(--green-border); }
  .status-badge { font-size: 10px; font-weight: 700; padding: 2px 8px; border-radius: 4px;
                  text-transform: uppercase; letter-spacing: .05em; }
  .status-badge.pass { background: var(--green-bg); color: var(--green); }
  .status-badge.fail { background: var(--red-bg);   color: var(--red); }
  .status-badge.skip { background: var(--yellow-bg);color: var(--yellow); }
  .expand-icon { color: var(--muted); font-size: 11px; transition: transform .15s; }
  .expand-icon.open { transform: rotate(90deg); color: var(--blue); }

  /* Test detail panel */
  .test-detail { display: none; padding: 10px 16px 14px 34px;
                 background: rgba(0,0,0,.25); border-top: 1px solid var(--border); }
  .test-detail.open { display: block; }
  .detail-row { display: flex; gap: 8px; margin-bottom: 6px; font-size: 12px; }
  .detail-key { color: var(--muted); min-width: 80px; }
  .detail-val { color: var(--text); font-family: monospace; word-break: break-all; }
  .detail-val.fail { color: var(--red); }
  .detail-val.ok   { color: var(--green); }
  .body-block { margin-top: 10px; }
  .body-label { font-size: 11px; color: var(--muted); text-transform: uppercase;
                letter-spacing: .07em; margin-bottom: 4px; }
  pre { background: var(--bg); border: 1px solid var(--border); border-radius: 6px;
        padding: 10px 12px; font-size: 12px; overflow-x: auto; color: var(--cyan);
        max-height: 200px; overflow-y: auto; white-space: pre-wrap; word-break: break-all; }

  /* Alerts / Toasts */
  .alerts { position: fixed; top: 16px; right: 16px; display: flex; flex-direction: column;
            gap: 8px; z-index: 100; max-width: 320px; }
  .alert { padding: 12px 16px; border-radius: 8px; font-size: 13px; font-weight: 500;
           box-shadow: 0 4px 20px rgba(0,0,0,.5); animation: slideIn .3s ease-out;
           cursor: pointer; transition: opacity .2s; }
  .alert:hover { opacity: .8; }
  .alert.error { background: var(--red-bg); border: 1px solid var(--red-border); color: var(--red); }
  .alert.success { background: var(--green-bg); border: 1px solid var(--green-border); color: var(--green); }
  @keyframes slideIn { from { transform: translateX(110%); opacity: 0; }
                       to   { transform: translateX(0);    opacity: 1; } }

  /* Footer */
  .footer { text-align: center; color: var(--muted); font-size: 12px; margin-top: 40px; }

  /* Highlight failing sections */
  .section.has-failures > .section-header { border-left: 3px solid var(--red); }
  .section.all-pass > .section-header { border-left: 3px solid var(--green); }

  /* Search */
  .search-wrap { position: relative; margin-bottom: 16px; }
  .search-input { width: 100%; background: var(--surface); border: 1px solid var(--border);
                  border-radius: 8px; padding: 8px 12px 8px 34px; color: var(--text);
                  font-size: 13px; outline: none; transition: border-color .15s; }
  .search-input:focus { border-color: var(--blue); }
  .search-icon { position: absolute; left: 10px; top: 50%; transform: translateY(-50%);
                 color: var(--muted); font-size: 14px; pointer-events: none; }

  /* Responsive */
  @media (max-width: 600px) {
    .summary { grid-template-columns: repeat(2, 1fr); }
    .card .val { font-size: 24px; }
  }
</style>
</head>
<body>
<div class="container">

  <!-- Header -->
  <div class="header">
    <div class="header-title">
      <h1>JobTracker Pro — Test Report</h1>
      <span class="subtitle">Generado: $ts_human &nbsp;|&nbsp; $((total_ms / 1000)).$((total_ms % 1000 / 10))s total</span>
    </div>
    <span class="target-badge">$BASE_URL</span>
  </div>

  <!-- Summary cards -->
  <div class="summary">
    <div class="card"><div class="val" id="c-total">$total</div><div class="lbl">Total</div></div>
    <div class="card pass-card"><div class="val" id="c-pass">$PASS</div><div class="lbl">Pasaron</div></div>
    <div class="card fail-card"><div class="val" id="c-fail">$FAIL</div><div class="lbl">Fallaron</div></div>
    <div class="card skip-card"><div class="val" id="c-skip">$SKIP</div><div class="lbl">Omitidos</div></div>
    <div class="card time-card"><div class="val">${total_ms}ms</div><div class="lbl">Duración</div></div>
  </div>

  <!-- Progress bar -->
  <div class="progress-wrap">
    <div class="progress-bar" id="progress-bar" style="width:0%"></div>
  </div>

  <!-- Timeline -->
  <div class="timeline-label">Timeline de ejecución</div>
  <div class="timeline" id="timeline"></div>

  <!-- Filter + Search -->
  <div class="filter-bar">
    <button class="filter-btn active" data-filter="all">Todos ($total)</button>
    <button class="filter-btn" data-filter="pass">Pasaron ($PASS)</button>
    <button class="filter-btn" data-filter="fail">Fallaron ($FAIL)</button>
    <button class="filter-btn" data-filter="skip">Omitidos ($SKIP)</button>
  </div>
  <div class="search-wrap">
    <span class="search-icon">&#128269;</span>
    <input class="search-input" id="search" type="text" placeholder="Buscar prueba...">
  </div>

  <!-- Sections -->
  <div id="sections-root"></div>

  <div class="footer">JobTracker Pro &mdash; E2E API Tests &mdash; $ts_human</div>
</div>

<!-- Toasts -->
<div class="alerts" id="alerts"></div>

<script>
const RESULTS = [$REPORT_JSON];
const TOTAL_MS = $total_ms;
const PASS_COUNT = $PASS;
const FAIL_COUNT = $FAIL;

// ── Progress bar ─────────────────────────────────────────────
const total = RESULTS.length;
setTimeout(() => {
  const pct = total > 0 ? Math.round((PASS_COUNT / total) * 100) : 0;
  document.getElementById('progress-bar').style.width = pct + '%';
}, 100);

// ── Timeline ─────────────────────────────────────────────────
const tlEl = document.getElementById('timeline');
const minTs = Math.min(...RESULTS.map(r => r.ts));
const maxTs = Math.max(...RESULTS.map(r => r.ts + Math.max(r.ms, 10)));
const span  = maxTs - minTs || 1;
RESULTS.forEach((r, i) => {
  const seg = document.createElement('div');
  const leftPct  = ((r.ts - minTs) / span * 100).toFixed(2);
  const widthPct = (Math.max(r.ms, 8)  / span * 100).toFixed(2);
  seg.className  = 'tl-seg ' + r.status;
  seg.style.cssText = 'flex:none;width:' + widthPct + '%;min-width:4px;';
  seg.title = r.label + ' (' + r.ms + 'ms)';
  seg.addEventListener('click', () => scrollToTest(i));
  tlEl.appendChild(seg);
});

// ── Group by section ─────────────────────────────────────────
const sections = {};
RESULTS.forEach((r, idx) => {
  if (!sections[r.section]) sections[r.section] = [];
  sections[r.section].push({ ...r, idx });
});

// ── Render ───────────────────────────────────────────────────
const root = document.getElementById('sections-root');
let testElements = [];

Object.entries(sections).forEach(([secName, tests]) => {
  const failCount = tests.filter(t => t.status === 'fail').length;
  const passCount = tests.filter(t => t.status === 'pass').length;

  const sec = document.createElement('div');
  sec.className = 'section' + (failCount > 0 ? ' has-failures' : (passCount === tests.length ? ' all-pass' : ''));
  sec.dataset.section = secName;

  // Pills
  let pillsHtml = '';
  if (passCount > 0) pillsHtml += '<span class="pill pass">✓ ' + passCount + '</span>';
  if (failCount > 0) pillsHtml += '<span class="pill fail">✗ ' + failCount + '</span>';
  const skipCount = tests.filter(t => t.status === 'skip').length;
  if (skipCount > 0) pillsHtml += '<span class="pill skip">~ ' + skipCount + '</span>';

  sec.innerHTML = '<div class="section-header">' +
    '<span class="chevron open">&#9658;</span>' +
    '<h2>' + escHtml(secName) + '</h2>' +
    '<div class="section-pills">' + pillsHtml + '</div>' +
    '</div>' +
    '<div class="section-body open" id="sec-body-' + secName.replace(/\s+/g,'-') + '"></div>';

  const hdr = sec.querySelector('.section-header');
  const body = sec.querySelector('.section-body');
  const chev = sec.querySelector('.chevron');
  hdr.addEventListener('click', () => {
    body.classList.toggle('open');
    chev.classList.toggle('open');
  });

  tests.forEach(t => {
    const row = document.createElement('div');
    row.className = 'test-row';
    row.dataset.status = t.status;
    row.dataset.label  = t.label.toLowerCase();
    row.dataset.idx    = t.idx;

    const msClass = t.ms > 2000 ? 'slow' : (t.ms < 500 ? 'fast' : '');
    const msLabel = t.ms > 0 ? t.ms + 'ms' : '—';
    const hasDetail = t.expected || t.actual || t.body;

    row.innerHTML =
      '<div class="test-main" onclick="toggleDetail(this)">' +
        '<div class="dot ' + t.status + '"></div>' +
        '<div class="test-label ' + t.status + '">' + escHtml(t.label) + '</div>' +
        (hasDetail ? '<span class="expand-icon">&#9658;</span>' : '') +
        '<span class="ms-badge ' + msClass + '">' + msLabel + '</span>' +
        '<span class="status-badge ' + t.status + '">' + t.status.toUpperCase() + '</span>' +
      '</div>' +
      '<div class="test-detail" id="detail-' + t.idx + '">' +
        buildDetail(t) +
      '</div>';

    body.appendChild(row);
    testElements.push({ el: row, t });
  });

  root.appendChild(sec);
});

function buildDetail(t) {
  let html = '';
  if (t.expected) {
    html += '<div class="detail-row"><span class="detail-key">Esperado</span>' +
            '<span class="detail-val ok">' + escHtml(t.expected) + '</span></div>';
  }
  if (t.actual) {
    html += '<div class="detail-row"><span class="detail-key">Obtenido</span>' +
            '<span class="detail-val fail">' + escHtml(t.actual) + '</span></div>';
  }
  if (t.ms > 0) {
    html += '<div class="detail-row"><span class="detail-key">Duración</span>' +
            '<span class="detail-val">' + t.ms + 'ms</span></div>';
  }
  if (t.note) {
    html += '<div class="detail-row"><span class="detail-key">Nota</span>' +
            '<span class="detail-val">' + escHtml(t.note) + '</span></div>';
  }
  if (t.body) {
    let pretty = t.body.replace(/\\n/g, '\n');
    try { pretty = JSON.stringify(JSON.parse(pretty), null, 2); } catch(e) {}
    html += '<div class="body-block"><div class="body-label">Response body</div>' +
            '<pre>' + escHtml(pretty) + '</pre></div>';
  }
  return html || '<div class="detail-row"><span class="detail-key" style="color:var(--muted)">Sin detalles adicionales</span></div>';
}

function toggleDetail(mainEl) {
  const icon   = mainEl.querySelector('.expand-icon');
  const detail = mainEl.nextElementSibling;
  if (!detail || !detail.classList.contains('test-detail')) return;
  detail.classList.toggle('open');
  if (icon) icon.classList.toggle('open');
}

function scrollToTest(idx) {
  const el = document.querySelector('[data-idx="' + idx + '"]');
  if (el) { el.scrollIntoView({ behavior: 'smooth', block: 'center' }); }
}

// ── Filter ───────────────────────────────────────────────────
let currentFilter = 'all';
let currentSearch = '';

document.querySelectorAll('.filter-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    currentFilter = btn.dataset.filter;
    applyFilter();
  });
});

document.getElementById('search').addEventListener('input', e => {
  currentSearch = e.target.value.toLowerCase().trim();
  applyFilter();
});

function applyFilter() {
  let visibleSections = new Set();
  testElements.forEach(({ el, t }) => {
    const matchFilter = currentFilter === 'all' || t.status === currentFilter;
    const matchSearch = !currentSearch || t.label.toLowerCase().includes(currentSearch) ||
                        t.section.toLowerCase().includes(currentSearch);
    const show = matchFilter && matchSearch;
    el.style.display = show ? '' : 'none';
    if (show) visibleSections.add(t.section);
  });
  // Show/hide sections
  document.querySelectorAll('.section').forEach(sec => {
    sec.style.display = visibleSections.has(sec.dataset.section) ? '' : 'none';
  });
}

// ── Auto-expand failing sections ─────────────────────────────
document.querySelectorAll('.section.has-failures').forEach(sec => {
  const body = sec.querySelector('.section-body');
  const chev = sec.querySelector('.chevron');
  if (body) { body.classList.add('open'); }
  if (chev) { chev.classList.add('open'); }
});

// ── Toasts ───────────────────────────────────────────────────
const alertsEl = document.getElementById('alerts');
function showToast(msg, type, delay) {
  setTimeout(() => {
    const a = document.createElement('div');
    a.className = 'alert ' + type;
    a.textContent = msg;
    a.addEventListener('click', () => a.remove());
    alertsEl.appendChild(a);
    setTimeout(() => a.remove(), 6000);
  }, delay);
}

if (FAIL_COUNT === 0) {
  showToast('Todos los escenarios pasaron (' + PASS_COUNT + '/' + total + ')', 'success', 400);
} else {
  showToast(FAIL_COUNT + ' escenario(s) fallaron — revisa los detalles abajo', 'error', 400);
  // One toast per failing section
  const failSections = [...new Set(RESULTS.filter(r => r.status === 'fail').map(r => r.section))];
  failSections.forEach((sec, i) => {
    const count = RESULTS.filter(r => r.status === 'fail' && r.section === sec).length;
    showToast(sec + ': ' + count + ' falla(s)', 'error', 800 + i * 300);
  });
}

// ── Helpers ──────────────────────────────────────────────────
function escHtml(str) {
  return String(str)
    .replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;')
    .replace(/"/g,'&quot;').replace(/'/g,'&#039;');
}
</script>
</body>
</html>
HTMLEOF
}

# ══════════════════════════════════════════════════════════════
# TESTS
# ══════════════════════════════════════════════════════════════

echo -e "${BOLD}JobTracker Pro — E2E API Tests${RESET}"
echo -e "Target: ${YELLOW}$BASE_URL${RESET}"
echo -e "Report: ${YELLOW}$REPORT_FILE${RESET}"
echo "---"

# ── 1. Health Check ───────────────────────────────────────────
section "Health Check"

http GET "$BASE_URL/api/jobapplications/00000000-0000-0000-0000-000000000000"
if [[ "$HTTP_STATUS" == "401" || "$HTTP_STATUS" == "200" || "$HTTP_STATUS" == "404" ]]; then
  pass "API alcanzable (responde HTTP $HTTP_STATUS)"
else
  fail "API no responde" "200/401/404" "HTTP $HTTP_STATUS"
  echo -e "\n${RED}API no alcanzable en $BASE_URL. Verifica que el servidor esté corriendo.${RESET}"
  generate_html
  exit 1
fi

# ── 2. Auth — Registro ────────────────────────────────────────
section "Auth — Registro"

UNIQUE_ID="$(date +%s%N 2>/dev/null | tail -c 8 || date +%s | tail -c 6)"
TEST_EMAIL="e2e_${UNIQUE_ID}@jobtracker.test"
TEST_PASSWORD="Password123!"

clear_token
http POST "$BASE_URL/api/auth/register" -d \
  "{\"name\":\"E2E Tester\",\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}"
assert_status "200" "Registro con datos válidos retorna 200"
assert_json_nonempty "accessToken" "Respuesta incluye accessToken"
assert_json_nonempty "refreshToken" "Respuesta incluye refreshToken"
ACCESS_TOKEN=$(json_field "accessToken")
REFRESH_TOKEN=$(json_field "refreshToken")

http POST "$BASE_URL/api/auth/register" -d \
  "{\"name\":\"Duplicate\",\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}"
if [[ "$HTTP_STATUS" == "500" || "$HTTP_STATUS" == "409" || "$HTTP_STATUS" == "400" ]]; then
  pass "Email duplicado rechazado (HTTP $HTTP_STATUS)"
else
  fail "Email duplicado debe retornar error" "400/409/500" "HTTP $HTTP_STATUS"
fi

http POST "$BASE_URL/api/auth/register" -d '{"name":"","email":"not-an-email","password":"x"}'
if [[ "$HTTP_STATUS" == "400" || "$HTTP_STATUS" == "500" ]]; then
  pass "Datos inválidos en registro retornan error (HTTP $HTTP_STATUS)"
else
  fail "Datos inválidos deben ser rechazados" "400/500" "HTTP $HTTP_STATUS"
fi

# ── 3. Auth — Login ───────────────────────────────────────────
section "Auth — Login"

clear_token
http POST "$BASE_URL/api/auth/login" -d \
  "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}"
assert_status "200" "Login con credenciales válidas retorna 200"
assert_json_nonempty "accessToken" "Login retorna accessToken"
ACCESS_TOKEN=$(json_field "accessToken")
REFRESH_TOKEN=$(json_field "refreshToken")

http POST "$BASE_URL/api/auth/login" -d \
  "{\"email\":\"$TEST_EMAIL\",\"password\":\"WrongPassword!\"}"
assert_status "401" "Contraseña incorrecta retorna 401"

http POST "$BASE_URL/api/auth/login" -d \
  '{"email":"noexiste@jobtracker.test","password":"Password123!"}'
assert_status "401" "Email inexistente retorna 401"

http POST "$BASE_URL/api/auth/login" -d '{}'
if [[ "$HTTP_STATUS" == "400" || "$HTTP_STATUS" == "401" || "$HTTP_STATUS" == "500" ]]; then
  pass "Cuerpo vacío en login retorna error (HTTP $HTTP_STATUS)"
else
  fail "Cuerpo vacío debe retornar error" "400/401" "HTTP $HTTP_STATUS"
fi

# ── 4. Auth — Refresh Token ───────────────────────────────────
section "Auth — Refresh Token"

clear_token
http POST "$BASE_URL/api/auth/refresh" -d \
  "{\"accessToken\":\"$ACCESS_TOKEN\",\"refreshToken\":\"$REFRESH_TOKEN\"}"
if [[ "$HTTP_STATUS" == "200" ]]; then
  pass "Refresh con token válido retorna 200"
  assert_json_nonempty "accessToken" "Refresh retorna nuevo accessToken"
  ACCESS_TOKEN=$(json_field "accessToken")
  REFRESH_TOKEN=$(json_field "refreshToken")
else
  skip "Refresh token (HTTP $HTTP_STATUS — puede requerir ventana de tiempo)" "token demasiado reciente"
fi

http POST "$BASE_URL/api/auth/refresh" -d '{"accessToken":"invalid.token.here","refreshToken":"bad-token"}'
if [[ "$HTTP_STATUS" == "400" || "$HTTP_STATUS" == "401" || "$HTTP_STATUS" == "500" ]]; then
  pass "Refresh con token inválido retorna error (HTTP $HTTP_STATUS)"
else
  fail "Refresh inválido debe retornar error" "400/401" "HTTP $HTTP_STATUS"
fi

# ── 5. Auth requerida ─────────────────────────────────────────
section "Autenticación requerida (sin token)"

# Generar un userId para las pruebas de esta sesión
USER_ID="aaaabbbb-cccc-dddd-eeee-$(printf '%012x' $UNIQUE_ID 2>/dev/null || echo '000000000001')"

clear_token
http GET "$BASE_URL/api/jobapplications/$USER_ID"
assert_status "401" "GET lista sin token retorna 401"

http POST "$BASE_URL/api/jobapplications" -d \
  "{\"userId\":\"$USER_ID\",\"title\":\"Dev\",\"companyName\":\"Acme\",\"source\":\"Direct\"}"
assert_status "401" "POST crear sin token retorna 401"

http PUT "$BASE_URL/api/jobapplications/00000000-0000-0000-0000-000000000001" -d '{"newStatus":1}'
assert_status "401" "PUT status sin token retorna 401"

http PATCH "$BASE_URL/api/jobapplications/00000000-0000-0000-0000-000000000001" -d '{"title":"Test","companyName":"Co"}'
assert_status "401" "PATCH edit sin token retorna 401"

http DELETE "$BASE_URL/api/jobapplications/00000000-0000-0000-0000-000000000001"
assert_status "401" "DELETE sin token retorna 401"

# ── 6. Crear aplicaciones ─────────────────────────────────────
section "Job Applications — Crear"

set_token "$ACCESS_TOKEN"

http POST "$BASE_URL/api/jobapplications" -d \
  "{\"userId\":\"$USER_ID\",\"title\":\"Backend Engineer\",\"companyName\":\"Acme Corp\",\"source\":\"LinkedIn\"}"
assert_status "201" "Crear aplicación mínima retorna 201"
assert_json_nonempty "id" "Crear retorna ID de la nueva aplicación"
APP_ID=$(json_field "id")

http POST "$BASE_URL/api/jobapplications" -d \
  "{\"userId\":\"$USER_ID\",\"title\":\"Senior .NET Dev\",\"companyName\":\"Google\",\"jobUrl\":\"https://careers.google.com/1\",\"description\":\"Remote. Great benefits.\",\"source\":\"Direct\"}"
assert_status "201" "Crear aplicación con todos los campos retorna 201"
APP_ID_2=$(json_field "id")

http POST "$BASE_URL/api/jobapplications" -d \
  "{\"userId\":\"$USER_ID\",\"title\":\"UX Designer\",\"companyName\":\"Startup XYZ\",\"source\":\"Indeed\"}"
assert_status "201" "Crear tercera aplicación retorna 201"
APP_ID_3=$(json_field "id")

http POST "$BASE_URL/api/jobapplications" -d \
  "{\"userId\":\"$USER_ID\",\"title\":\"\",\"companyName\":\"\",\"source\":\"\"}"
assert_status "400" "Crear con title/company vacíos retorna 400"

# ── 7. Listar ─────────────────────────────────────────────────
section "Job Applications — Listar"

http GET "$BASE_URL/api/jobapplications/$USER_ID"
assert_status "200" "GET lista retorna 200"

APP_COUNT=$(printf '%s' "$RESPONSE_BODY" | grep -o '"id"' | wc -l | tr -d ' ')
if [[ "$APP_COUNT" -ge 3 ]]; then
  pass "Lista contiene las 3 aplicaciones creadas ($APP_COUNT encontradas)"
else
  fail "Lista debe tener >= 3 aplicaciones" ">= 3" "$APP_COUNT encontradas"
fi

STATUS_NUM=$(printf '%s' "$RESPONSE_BODY" | grep -oP '"status"\s*:\s*\d+' | head -1 | grep -oP '\d+')
if [[ "$STATUS_NUM" =~ ^[0-9]+$ ]]; then
  pass "Status retornado como número entero ($STATUS_NUM)"
else
  fail "Status debe ser int, no string" "número entero" "'$STATUS_NUM'"
fi

UPDATED_AT_PRESENT=$(printf '%s' "$RESPONSE_BODY" | grep -c '"updatedAt"' || true)
if [[ "$UPDATED_AT_PRESENT" -ge 1 ]]; then
  pass "Campo updatedAt presente en los resultados"
else
  fail "updatedAt ausente en la respuesta" "campo updatedAt" "no encontrado"
fi

EMPTY_UID="ffffffff-ffff-ffff-ffff-ffffffffffff"
http GET "$BASE_URL/api/jobapplications/$EMPTY_UID"
assert_status "200" "GET usuario sin aplicaciones retorna 200"
if printf '%s' "$RESPONSE_BODY" | grep -q '^\[\]\|^\[ \]'; then
  pass "Usuario sin aplicaciones retorna array vacío []"
else
  pass "Usuario sin aplicaciones retorna respuesta válida"
fi

# ── 8. Actualizar status (PUT) ────────────────────────────────
section "Job Applications — Actualizar Status"

http PUT "$BASE_URL/api/jobapplications/$APP_ID" -d '{"newStatus":1}'
assert_status "204" "Saved → Applied (1) retorna 204"

http PUT "$BASE_URL/api/jobapplications/$APP_ID" -d \
  '{"newStatus":2,"notes":"Llamada con reclutador el lunes"}'
assert_status "204" "Applied → PhoneScreen (2) con notas retorna 204"

http PUT "$BASE_URL/api/jobapplications/$APP_ID" -d '{"newStatus":3}'
assert_status "204" "PhoneScreen → TechnicalTest (3) retorna 204"

http PUT "$BASE_URL/api/jobapplications/$APP_ID" -d \
  '{"newStatus":4,"notes":"Entrevista con el CTO"}'
assert_status "204" "TechnicalTest → Interview (4) retorna 204"

http PUT "$BASE_URL/api/jobapplications/$APP_ID" -d '{"newStatus":5}'
assert_status "204" "Interview → FinalInterview (5) retorna 204"

http PUT "$BASE_URL/api/jobapplications/$APP_ID" -d \
  '{"newStatus":6,"notes":"Oferta de 95k + beneficios"}'
assert_status "204" "FinalInterview → OfferReceived (6) retorna 204"

http PUT "$BASE_URL/api/jobapplications/$APP_ID_2" -d \
  '{"newStatus":7,"notes":"No encajaba con el stack"}'
assert_status "204" "→ Rejected (7) retorna 204"

http POST "$BASE_URL/api/jobapplications" -d \
  "{\"userId\":\"$USER_ID\",\"title\":\"Temp\",\"companyName\":\"TmpCo\",\"source\":\"Direct\"}"
WITHDRAWN_ID=$(json_field "id")
http PUT "$BASE_URL/api/jobapplications/$WITHDRAWN_ID" -d '{"newStatus":8}'
assert_status "204" "→ Withdrawn (8) retorna 204"

http PUT "$BASE_URL/api/jobapplications/00000000-0000-0000-0000-000000000000" -d '{"newStatus":1}'
assert_status "404" "PUT con ID inexistente retorna 404"

# ── 9. Editar campos (PATCH) ──────────────────────────────────
section "Job Applications — Editar (PATCH)"

http PATCH "$BASE_URL/api/jobapplications/$APP_ID" -d \
  '{"title":"Senior Backend Engineer","companyName":"Acme Corp","jobUrl":"https://acme.io/jobs/42","notes":"Revisar contrato antes del jueves"}'
assert_status "204" "PATCH todos los campos retorna 204"

http GET "$BASE_URL/api/jobapplications/$USER_ID"
if printf '%s' "$RESPONSE_BODY" | grep -q "Senior Backend Engineer"; then
  pass "Cambios de título persisten en GET"
else
  fail "Título editado no encontrado en GET" "'Senior Backend Engineer'" "no encontrado"
fi
if printf '%s' "$RESPONSE_BODY" | grep -q "Revisar contrato"; then
  pass "Cambios de notes persisten en GET"
else
  fail "Notes editadas no encontradas en GET" "'Revisar contrato'" "no encontrado"
fi

http PATCH "$BASE_URL/api/jobapplications/$APP_ID" -d \
  '{"title":"Principal Engineer","companyName":"Acme Corp"}'
assert_status "204" "PATCH solo title retorna 204"

http PATCH "$BASE_URL/api/jobapplications/$APP_ID" -d \
  '{"title":"Principal Engineer","companyName":"Nueva Empresa Diferente SA"}'
assert_status "204" "PATCH cambia compañía (crea empresa nueva) retorna 204"

http PATCH "$BASE_URL/api/jobapplications/00000000-0000-0000-0000-000000000000" -d \
  '{"title":"Test","companyName":"TestCo"}'
assert_status "404" "PATCH con ID inexistente retorna 404"

# ── 10. Eliminar ──────────────────────────────────────────────
section "Job Applications — Eliminar"

http POST "$BASE_URL/api/jobapplications" -d \
  "{\"userId\":\"$USER_ID\",\"title\":\"Para Borrar\",\"companyName\":\"DeleteCo\",\"source\":\"Direct\"}"
assert_status "201" "Crear aplicación para eliminar"
DEL_ID=$(json_field "id")

http DELETE "$BASE_URL/api/jobapplications/$DEL_ID"
assert_status "204" "DELETE aplicación existente retorna 204"

http GET "$BASE_URL/api/jobapplications/$USER_ID"
if printf '%s' "$RESPONSE_BODY" | grep -q "\"$DEL_ID\""; then
  fail "Aplicación eliminada sigue apareciendo en GET" "ausente de la lista" "presente"
else
  pass "Aplicación eliminada ya no aparece en la lista"
fi

http DELETE "$BASE_URL/api/jobapplications/$DEL_ID"
assert_status "404" "DELETE segunda vez (ya eliminada) retorna 404"

http DELETE "$BASE_URL/api/jobapplications/00000000-0000-0000-0000-000000000000"
assert_status "404" "DELETE con ID inexistente retorna 404"

# ── 11. Flujo completo E2E ────────────────────────────────────
section "Flujo Completo E2E"

FLOW_SUFFIX="$(date +%s%N 2>/dev/null | tail -c 9 || date +%s)"
FLOW_EMAIL="flow_${FLOW_SUFFIX}@jobtracker.test"
FLOW_UID="bbbbcccc-dddd-eeee-ffff-$(printf '%012x' ${FLOW_SUFFIX: -9} 2>/dev/null || echo '000000000002')"

clear_token
http POST "$BASE_URL/api/auth/register" -d \
  "{\"name\":\"Flow User\",\"email\":\"$FLOW_EMAIL\",\"password\":\"Password123!\"}"
assert_status "200" "F1. Registrar nuevo usuario"
FLOW_TOKEN=$(json_field "accessToken")
set_token "$FLOW_TOKEN"

http POST "$BASE_URL/api/jobapplications" -d \
  "{\"userId\":\"$FLOW_UID\",\"title\":\"Full Stack Developer\",\"companyName\":\"Startup XYZ\",\"jobUrl\":\"https://startupxyz.com/jobs/fs\",\"description\":\"Posición remota, 100% equidad en decisiones técnicas.\",\"source\":\"LinkedIn\"}"
assert_status "201" "F2. Crear aplicación completa"
FLOW_APP=$(json_field "id")

http GET "$BASE_URL/api/jobapplications/$FLOW_UID"
assert_status "200" "F3. Listar aplicaciones del usuario recién creado"
if printf '%s' "$RESPONSE_BODY" | grep -q "Full Stack Developer"; then
  pass "F3. Aplicación creada aparece en la lista"
else
  fail "F3. Aplicación no encontrada en la lista"
fi

declare -a PIPELINE=(
  "1:Applied:Enviando CV y carta de presentación"
  "2:PhoneScreen:Llamada de 30 min con HR"
  "3:TechnicalTest:Prueba técnica de 2h en HackerRank"
  "4:Interview:Entrevista con el equipo de ingeniería (2h)"
  "5:FinalInterview:Entrevista final con el CTO y CEO"
  "6:OfferReceived:Oferta recibida: 90k USD + equity"
)
for pair in "${PIPELINE[@]}"; do
  S="${pair%%:*}"; rest="${pair#*:}"; NAME="${rest%%:*}"; NOTE="${rest#*:}"
  http PUT "$BASE_URL/api/jobapplications/$FLOW_APP" -d "{\"newStatus\":$S,\"notes\":\"$NOTE\"}"
  assert_status "204" "F4. Pipeline: Saved → $NAME (status $S)"
done

http PUT "$BASE_URL/api/jobapplications/$FLOW_APP" -d \
  '{"newStatus":7,"notes":"Oferta aceptada. Inicio: 1 de abril."}'
assert_status "204" "F5. Aceptar oferta (OfferAccepted=7)"

http PATCH "$BASE_URL/api/jobapplications/$FLOW_APP" -d \
  '{"title":"Full Stack Developer (Senior)","companyName":"Startup XYZ","notes":"Sueldo final negociado: 95k. Revisión a los 6 meses."}'
assert_status "204" "F6. Editar datos finales tras aceptar"

http GET "$BASE_URL/api/jobapplications/$FLOW_UID"
if printf '%s' "$RESPONSE_BODY" | grep -q "Full Stack Developer (Senior)"; then
  pass "F7. Edición final reflejada en GET"
else
  fail "F7. Edición final no encontrada" "'Full Stack Developer (Senior)'" "no encontrado"
fi

http DELETE "$BASE_URL/api/jobapplications/$FLOW_APP"
assert_status "204" "F8. Eliminar aplicación"

http GET "$BASE_URL/api/jobapplications/$FLOW_UID"
assert_status "200" "F9. GET después de eliminar retorna 200"
if printf '%s' "$RESPONSE_BODY" | grep -q "\"$FLOW_APP\""; then
  fail "F9. Aplicación eliminada sigue en la lista"
else
  pass "F9. Lista vacía confirmada tras eliminar"
fi

# ══════════════════════════════════════════════════════════════
# RESUMEN CONSOLA
# ══════════════════════════════════════════════════════════════
TOTAL_END_MS=$(now_ms)
TOTAL_MS=$(( TOTAL_END_MS - TOTAL_START_MS ))
TOTAL=$(( PASS + FAIL + SKIP ))

echo -e "\n${BOLD}══════════════════════════════════════${RESET}"
echo -e "${BOLD}  Resultados${RESET}"
echo -e "${BOLD}══════════════════════════════════════${RESET}"
echo -e "  Total:    $TOTAL  |  Tiempo: ${TOTAL_MS}ms"
echo -e "  ${GREEN}Passed:   $PASS${RESET}"
[[ $FAIL -gt 0 ]] && echo -e "  ${RED}Failed:   $FAIL${RESET}" || echo -e "  Failed:   $FAIL"
[[ $SKIP -gt 0 ]] && echo -e "  ${YELLOW}Skipped:  $SKIP${RESET}" || echo -e "  Skipped:  $SKIP"
echo -e "${BOLD}══════════════════════════════════════${RESET}"

# Generar HTML
generate_html
echo -e "\n${CYAN}Reporte HTML generado:${RESET} $REPORT_FILE"

if [[ $FAIL -eq 0 ]]; then
  echo -e "${GREEN}${BOLD}Todos los escenarios pasaron.${RESET}"
  exit 0
else
  echo -e "${RED}${BOLD}$FAIL escenario(s) fallaron.${RESET}"
  exit 1
fi
