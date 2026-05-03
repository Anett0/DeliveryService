import http from 'k6/http';
import { check, fail, sleep } from 'k6';

const BASE_URL = (__ENV.BASE_URL || 'http://localhost:5166').replace(/\/$/, '');
const TRACKING_CODE = __ENV.TRACKING_CODE;

export const options = {
  stages: [
    { duration: __ENV.RAMP_UP || '30s', target: Number(__ENV.TRACKING_VUS || 20) },
    { duration: __ENV.HOLD_FOR || '1m', target: Number(__ENV.TRACKING_VUS || 20) },
    { duration: __ENV.RAMP_DOWN || '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.01'],
    checks: ['rate>0.99'],
  },
};

export function setup() {
  if (!TRACKING_CODE) {
    fail('Set TRACKING_CODE to an existing package tracking code before running load_test.js.');
  }

  return { trackingCode: TRACKING_CODE };
}

export default function (data) {
  const response = http.get(`${BASE_URL}/api/packages/${data.trackingCode}`);

  check(response, {
    'status is 200': (res) => res.status === 200,
    'tracking code matches': (res) => res.json('trackingCode') === data.trackingCode,
  });

  sleep(Number(__ENV.SLEEP_SECONDS || 1));
}
