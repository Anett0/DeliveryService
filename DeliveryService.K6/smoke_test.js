import http from 'k6/http';
import { check, fail } from 'k6';

const BASE_URL = (__ENV.BASE_URL || 'http://localhost:5166').replace(/\/$/, '');
const TRACKING_CODE = __ENV.TRACKING_CODE;

export const options = {
  vus: 1,
  iterations: 1,
  thresholds: {
    checks: ['rate==1.0'],
    http_req_failed: ['rate==0'],
    http_req_duration: ['p(95)<1000'],
  },
};

export function setup() {
  if (!TRACKING_CODE) {
    fail('Set TRACKING_CODE to an existing package tracking code before running smoke_test.js.');
  }

  return { trackingCode: TRACKING_CODE };
}

export default function (data) {
  const packageResponse = http.get(`${BASE_URL}/api/packages/${data.trackingCode}`);

  check(packageResponse, {
    'tracking status is 200': (response) => response.status === 200,
    'tracking code matches': (response) => response.json('trackingCode') === data.trackingCode,
  });

  const updatesResponse = http.get(`${BASE_URL}/api/packages/${data.trackingCode}/updates`);

  check(updatesResponse, {
    'updates status is 200': (response) => response.status === 200,
    'updates response is an array': (response) => Array.isArray(response.json()),
  });
}
