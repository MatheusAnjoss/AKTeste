const el = id => document.getElementById(id);

const path = location.pathname;
const API_BASE = `${location.origin}/api/weather`;
console.debug('API_BASE =', API_BASE);

let capitals = [];
let selected = { city: 'São Paulo', state: 'SP' };

async function fetchJSON(url) {
  try {
    const r = await fetch(url);
    if (!r.ok) {
      console.error(`Request failed ${r.status}: ${url}`);
      return null;
    }
    return await r.json();
  } catch (err) {
    console.error(`Fetch error for ${url}`, err);
    return null;
  }
}

const fallbackCapitals = [
  { city: "Rio Branco", state: "AC" }, { city: "Maceió", state: "AL" }, { city: "Macapá", state: "AP" },
  { city: "Manaus", state: "AM" }, { city: "Salvador", state: "BA" }, { city: "Fortaleza", state: "CE" },
  { city: "Brasília", state: "DF" }, { city: "Vitória", state: "ES" }, { city: "Goiânia", state: "GO" },
  { city: "São Luís", state: "MA" }, { city: "Cuiabá", state: "MT" }, { city: "Campo Grande", state: "MS" },
  { city: "Belo Horizonte", state: "MG" }, { city: "Belém", state: "PA" }, { city: "João Pessoa", state: "PB" },
  { city: "Curitiba", state: "PR" }, { city: "Recife", state: "PE" }, { city: "Teresina", state: "PI" },
  { city: "Rio de Janeiro", state: "RJ" }, { city: "Natal", state: "RN" }, { city: "Porto Alegre", state: "RS" },
  { city: "Porto Velho", state: "RO" }, { city: "Boa Vista", state: "RR" }, { city: "Florianópolis", state: "SC" },
  { city: "São Paulo", state: "SP" }, { city: "Aracaju", state: "SE" }, { city: "Palmas", state: "TO" }
];

function fmtDate(d) {
  return d.toISOString().slice(0, 10);
}

function setTodayRange() {
  const now = new Date();
  el('startDate').value = fmtDate(now);
  el('endDate').value = fmtDate(now);
}

function initDates() {
  const end = new Date();
  const start = new Date(end);
  start.setDate(end.getDate() - 7);
  el('startDate').value = fmtDate(start);
  el('endDate').value = fmtDate(end);
}

function selectDefault() {
  const idx = capitals.findIndex(c => c.city === 'São Paulo' || c.City === 'São Paulo');
  if (idx >= 0) {
    const cap = capitals[idx];
    selected.city = cap.city ?? cap.City;
    selected.state = cap.state ?? cap.State;
    el('citySelect').value = `${selected.city}|${selected.state}`;
  }
}

async function loadCapitals() {
  let data = await fetchJSON(`${API_BASE}/capitals`);
  if (!data || !Array.isArray(data) || data.length === 0) {
    console.warn('Using fallback capitals list');
    data = fallbackCapitals;
  }

  capitals = data;

  const sel = el('citySelect');
  sel.innerHTML = '';

  const items = capitals.map(c => ({
    city: c.city ?? c.City,
    state: c.state ?? c.State
  })).filter(c => c.city && c.state);

  items.sort((a, b) => a.city.localeCompare(b.city, 'pt-BR'));

  items.forEach(({ city, state }) => {
    const opt = document.createElement('option');
    opt.value = `${city}|${state}`;
    opt.textContent = `${city} - ${state}`;
    sel.appendChild(opt);
  });

  selectDefault();
}

function readSelected() {
  const v = el('citySelect').value;
  const [city, state] = v.split('|');
  selected = { city, state };
}

async function loadStats(dateStr) {
  const url = `${API_BASE}/statistics/${encodeURIComponent(selected.city)}/${encodeURIComponent(selected.state)}?date=${dateStr}`;
  const stats = await fetchJSON(url);
  if (!stats) {
    el('statMaxTemp').textContent = '-';
    el('statMinTemp').textContent = '-';
    el('statAvgTemp').textContent = '-';
    el('statAvgHumidity').textContent = '-';
    el('statAvgPressure').textContent = '-';
    el('statAvgWind').textContent = '-';
    return;
  }
  el('statMaxTemp').textContent = stats.maxTemperature?.toFixed(1);
  el('statMinTemp').textContent = stats.minTemperature?.toFixed(1);
  el('statAvgTemp').textContent = stats.averageTemperature?.toFixed(1);
  el('statAvgHumidity').textContent = stats.averageHumidity?.toFixed(1);
  el('statAvgPressure').textContent = stats.averagePressure?.toFixed(1);
  el('statAvgWind').textContent = stats.averageWindSpeed?.toFixed(1);
}

async function loadCurrent() {
  let data = await fetchJSON(`${API_BASE}/latest/${encodeURIComponent(selected.city)}/${encodeURIComponent(selected.state)}`);
  if (!data) {
    data = await fetchJSON(`${API_BASE}/current/${encodeURIComponent(selected.city)}/${encodeURIComponent(selected.state)}`);
  }
  if (!data) {
    console.warn('No current weather available');
    return;
  }

  el('curCity').textContent = `${data.city} - ${data.state}`;
  el('curDesc').textContent = data.description || data.weatherDescription || '-';
  el('curTemp').textContent = `${data.temperature?.toFixed?.(1) ?? '-'} °C`;
  el('curWind').textContent = `${data.windSpeed?.toFixed?.(1) ?? '-'} m/s (${data.windDirection ?? '-'}°)`;
  el('curClouds').textContent = `${data.cloudiness ?? '-'} %`;
  const ts = data.timestamp ? new Date(data.timestamp) : new Date();
  el('curTs').textContent = ts.toLocaleString();
}

async function loadChartData(startStr, endStr) {
  const startISO = `${startStr}T00:00:00`;
  const endISO = `${endStr}T23:59:59`;
  const qs = `?startDate=${encodeURIComponent(startISO)}&endDate=${encodeURIComponent(endISO)}`;

  const urlWithState = `${API_BASE}/chart-data/${encodeURIComponent(selected.city)}/${encodeURIComponent(selected.state)}${qs}`;
  console.debug('GET', urlWithState);
  let chart = await fetchJSON(urlWithState);

  if (!chart) {
    const urlCityOnly = `${API_BASE}/chart-data/${encodeURIComponent(selected.city)}${qs}`;
    console.debug('GET (fallback)', urlCityOnly);
    chart = await fetchJSON(urlCityOnly);
  }

  if (!chart || !Array.isArray(chart.labels) || chart.labels.length === 0) {
    const curUrl = `${API_BASE}/current/${encodeURIComponent(selected.city)}/${encodeURIComponent(selected.state)}`;
    console.debug('GET (current as single point)', curUrl);
    const current = await fetchJSON(curUrl);
    if (current) {
      const ts = current.timestamp ? new Date(current.timestamp) : new Date();
      return {
        labels: [ts.toLocaleString()],
        temperatureData: [{
          temperature: current.temperature ?? null,
          minTemp: current.minTemperature ?? current.temperature ?? null,
          maxTemp: current.maxTemperature ?? current.temperature ?? null
        }],
        humidityData: [{
          humidity: current.humidity ?? null
        }]
      };
    }
    return { labels: [], temperatureData: [], humidityData: [] };
  }

  return chart;
}

function clearCanvas(c) {
  const ctx = c.getContext('2d');
  ctx.clearRect(0, 0, c.width, c.height);
}

function drawAxes(ctx, w, h, padding, color = '#334155') {
  ctx.strokeStyle = color;
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(padding, h - padding);
  ctx.lineTo(w - padding, h - padding);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(padding, padding);
  ctx.lineTo(padding, h - padding);
  ctx.stroke();
}

function scale(value, min, max, outMin, outMax) {
  if (!Number.isFinite(value) || !Number.isFinite(min) || !Number.isFinite(max)) return outMin;
  if (max - min === 0) return outMin;
  return outMin + (value - min) * (outMax - outMin) / (max - min);
}

function drawLine(ctx, points, color, width = 2) {
  if (points.length < 2) return;
  ctx.strokeStyle = color;
  ctx.lineWidth = width;
  ctx.beginPath();
  ctx.moveTo(points[0].x, points[0].y);
  for (let i = 1; i < points.length; i++) {
    ctx.lineTo(points[i].x, points[i].y);
  }
  ctx.stroke();
}

function drawDots(ctx, points, color) {
  ctx.fillStyle = color;
  points.forEach(p => {
    ctx.beginPath();
    ctx.arc(p.x, p.y, 2, 0, Math.PI * 2);
    ctx.fill();
  });
}

function drawLabels(ctx, labels, w, h, padding) {
  ctx.fillStyle = '#94a3b8';
  ctx.font = '10px system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif';
  const step = Math.max(1, Math.floor(labels.length / 6));
  for (let i = 0; i < labels.length; i += step) {
    const x = scale(i, 0, labels.length - 1, padding, w - padding);
    ctx.fillText(labels[i], x - 20, h - padding + 12);
  }
}

function drawTemperatureChart(canvas, labels, temps, mins, maxs) {
  const ctx = canvas.getContext('2d');
  const w = canvas.width = canvas.clientWidth * window.devicePixelRatio;
  const h = canvas.height = canvas.clientHeight * window.devicePixelRatio;
  const pad = 40;

  clearCanvas(canvas);
  drawAxes(ctx, w, h, pad);

  if (temps.length === 0) return;

  const all = [...temps, ...mins, ...maxs].filter(v => Number.isFinite(v));
  if (all.length === 0) return;

  const vMin = Math.min(...all);
  const vMax = Math.max(...all);

  const points = temps.map((v, i) => ({
    x: scale(i, 0, temps.length - 1, pad, w - pad),
    y: scale(v, vMin, vMax, h - pad, pad)
  }));
  const pointsMin = mins.map((v, i) => ({
    x: scale(i, 0, mins.length - 1, pad, w - pad),
    y: scale(v, vMin, vMax, h - pad, pad)
  }));
  const pointsMax = maxs.map((v, i) => ({
    x: scale(i, 0, maxs.length - 1, pad, w - pad),
    y: scale(v, vMin, vMax, h - pad, pad)
  }));

  drawLine(ctx, pointsMin, '#22d3ee', 1);
  drawLine(ctx, pointsMax, '#f97316', 1);
  drawLine(ctx, points, '#60a5fa', 2);
  drawDots(ctx, points, '#60a5fa');
  drawLabels(ctx, labels, w, h, pad);
}

function drawHumidityChart(canvas, labels, hums) {
  const ctx = canvas.getContext('2d');
  const w = canvas.width = canvas.clientWidth * window.devicePixelRatio;
  const h = canvas.height = canvas.clientHeight * window.devicePixelRatio;
  const pad = 40;

  clearCanvas(canvas);
  drawAxes(ctx, w, h, pad);

  if (hums.length === 0) return;

  const vMin = 0;
  const vMax = 100;

  const points = hums.map((v, i) => ({
    x: scale(i, 0, hums.length - 1, pad, w - pad),
    y: scale(v, vMin, vMax, h - pad, pad)
  }));

  drawLine(ctx, points, '#34d399', 2);
  drawDots(ctx, points, '#34d399');
  drawLabels(ctx, labels, w, h, pad);
}

async function applyFilters() {
  readSelected();
  let start = el('startDate').value;
  let end = el('endDate').value;

  if (!start || !end) {
    initDates();
    start = el('startDate').value;
    end = el('endDate').value;
  }

  if (start > end) {
    const tmp = start;
    start = end;
    end = tmp;
    el('startDate').value = start;
    el('endDate').value = end;
  }

  await loadCurrent();
  await loadStats(end || fmtDate(new Date()));

  const chart = await loadChartData(start, end);
  const labels = (chart.labels ?? []).map(l => l);
  const t = (chart.temperatureData ?? []).map(p => +p.temperature);
  const tMin = (chart.temperatureData ?? []).map(p => +p.minTemp);
  const tMax = (chart.temperatureData ?? []).map(p => +p.maxTemp);
  const hum = (chart.humidityData ?? []).map(p => +p.humidity);

  drawTemperatureChart(el('tempChart'), labels, t, tMin, tMax);
  drawHumidityChart(el('humidityChart'), labels, hum);

  if ((labels?.length ?? 0) === 0) {
    console.warn('Sem dados para o período selecionado.');
  }
}

function bindUI() {
  el('citySelect').addEventListener('change', applyFilters);
  el('btnApply').addEventListener('click', applyFilters);
  el('btnToday').addEventListener('click', () => {
    setTodayRange();
    applyFilters();
  });

  window.addEventListener('resize', () => {
    applyFilters();
  });
}

(async function init() {
  await loadCapitals();
  initDates();
  bindUI();
  await applyFilters();
})();

