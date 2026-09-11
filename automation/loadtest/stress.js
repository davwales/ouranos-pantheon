// k6 stress test for the Ouranos Pantheon gateway.
//
// Read-only endpoints only, so it is safe to point at production. All knobs
// are env vars so the same script covers a smoke pass and a full stress run.
//
//   Smoke:  PROFILE=smoke k6 run automation/loadtest/stress.js
//   Stress: k6 run automation/loadtest/stress.js
//   Tuned:  BASE_URL=... MAX_VUS=200 HOLD=5m k6 run automation/loadtest/stress.js
//
// With the live web dashboard:  K6_WEB_DASHBOARD=true k6 run automation/loadtest/stress.js
//
// Thresholds define the service level you consider acceptable. During the
// ramp past the target load, a threshold start failing is the breaking point.

import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://ouranos.local:8300';
const MARKET_ID = __ENV.MARKET_ID || 'd71d7207-e30b-404f-8797-0148ad88cf9e';
const SYMBOL_ID = __ENV.SYMBOL_ID || 'ec36f592-e83a-4f16-bdac-af7f691bedf3';
const MAX_VUS = Number(__ENV.MAX_VUS || 100);
const PEAK_HOLD = __ENV.HOLD || '4m';
const PROFILE = __ENV.PROFILE || 'stress';
const REQUEST_TIMEOUT = __ENV.TIMEOUT || '15s';
const INCLUDE_RECOMMENDATIONS = __ENV.INCLUDE_RECOMMENDATIONS === 'true';

export const options = {
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<800', 'p(99)<1500'],
    'http_req_duration{endpoint:overview}': ['p(95)<1000'],
    'http_req_duration{endpoint:symbol-summary}': ['p(95)<800'],
    'http_req_duration{endpoint:volume-heatmap}': ['p(95)<800'],
    checks: ['rate>0.95'],
  },
  scenarios: {
    stress: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: buildStages(),
      gracefulRampDown: '30s',
    },
  },
};

function buildStages() {
  if (PROFILE === 'smoke') {
    return [{ duration: '30s', target: 2 }];
  }

  return [
    { duration: '1m', target: Math.ceil(MAX_VUS * 0.25) },
    { duration: '1m', target: Math.ceil(MAX_VUS * 0.5) },
    { duration: '2m', target: MAX_VUS },
    { duration: PEAK_HOLD, target: MAX_VUS },
    { duration: '1m', target: Math.ceil(MAX_VUS * 1.5) },
    { duration: '2m', target: Math.ceil(MAX_VUS * 1.5) },
    { duration: '1m', target: 0 },
  ];
}

const params = {
  timeout: REQUEST_TIMEOUT,
  headers: { 'User-Agent': 'k6-stress (ouranos-pantheon)' },
};

export function setup() {
  const marketId = collectIds(safeGetJson('/api/plutus/markets'))[0] || MARKET_ID;
  const symbolIds = collectIds(safeGetJson(`/api/plutus/symbols?filter=MarketId:eq:${marketId}&take=25`));
  if (symbolIds.length === 0) {
    symbolIds.push(SYMBOL_ID);
  }

  let strategyId = null;
  const strategies = safeGetJson(`/api/plutus/strategies?marketId=${marketId}`);
  const strategyIds = collectIds(strategies);
  if (strategyIds.length > 0) {
    strategyId = strategyIds[0];
  }

  return { marketId, symbolIds, strategyId };
}

export default function (data) {
  const picks = pickRequests(3);
  for (const entry of picks) {
    const res = httpGet(entry.endpoint, entry.path(data));
    check(res, { 'status is 200': (r) => r.status === 200 });
    sleep(0.3 + Math.random() * 1.2);
  }

  if (INCLUDE_RECOMMENDATIONS && data.strategyId) {
    const res = httpGet(
      'recommendations',
      `/api/plutus/strategies/${data.strategyId}/recommendations`,
    );
    check(res, { 'status is 200': (r) => r.status === 200 });
  }

  sleep(1 + Math.random() * 2);
}

const PLAN = [
  { weight: 5, endpoint: 'health', path: () => '/health' },
  { weight: 10, endpoint: 'markets', path: () => '/api/plutus/markets' },
  {
    weight: 15,
    endpoint: 'symbols',
    path: (d) => `/api/plutus/symbols?filter=MarketId:eq:${d.marketId}&take=25`,
  },
  {
    weight: 20,
    endpoint: 'overview',
    path: (d) => `/api/plutus/markets/${d.marketId}/overview?timeFrame=OneHour`,
  },
  {
    weight: 15,
    endpoint: 'signal-rankings',
    path: (d) => `/api/plutus/markets/${d.marketId}/signal-rankings`,
  },
  {
    weight: 10,
    endpoint: 'volume-heatmap',
    path: (d) => `/api/plutus/markets/${d.marketId}/volume-heatmap?lookbackWeeks=4`,
  },
  {
    weight: 15,
    endpoint: 'trades',
    path: (d) =>
      `/api/plutus/trades?filter=symbolId:eq:${pick(d.symbolIds)}&skip=0&take=20&sortField=timestamp&sortDirection=desc&timeFrame=OneYear`,
  },
  {
    weight: 10,
    endpoint: 'symbol-summary',
    path: (d) => `/api/plutus/symbols/${pick(d.symbolIds)}/summary`,
  },
  {
    weight: 5,
    endpoint: 'strategies',
    path: (d) => `/api/plutus/strategies?marketId=${d.marketId}`,
  },
  {
    weight: 10,
    endpoint: 'forecasts',
    path: (d) =>
      `/api/plutus/markets/${d.marketId}/forecasts?filter=symbolId:eq:${pick(d.symbolIds)}&skip=0&take=20`,
  },
];

function httpGet(endpoint, path) {
  return http.get(`${BASE_URL}${path}`, { ...params, tags: { endpoint } });
}

function pickRequests(count) {
  const picks = [];
  const remaining = PLAN.slice();

  for (let i = 0; i < count && remaining.length > 0; i++) {
    const total = remaining.reduce((sum, entry) => sum + entry.weight, 0);
    let roll = Math.random() * total;

    for (let j = 0; j < remaining.length; j++) {
      roll -= remaining[j].weight;
      if (roll <= 0) {
        picks.push(remaining[j]);
        remaining.splice(j, 1);
        break;
      }
    }
  }

  return picks;
}

function safeGetJson(path) {
  const res = http.get(`${BASE_URL}${path}`, params);
  if (res.status !== 200) {
    return null;
  }

  try {
    return res.json();
  } catch {
    return null;
  }
}

function collectIds(payload) {
  const items = Array.isArray(payload) ? payload : (payload && payload.items) || [];
  return items.map((item) => item && item.id).filter(Boolean);
}

function pick(list) {
  return list[Math.floor(Math.random() * list.length)];
}
