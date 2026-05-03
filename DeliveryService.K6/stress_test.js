import http from 'k6/http';
import { check, fail } from 'k6';

const BASE_URL = (__ENV.BASE_URL || 'http://localhost:5166').replace(/\/$/, '');
const PACKAGE_IDS = (__ENV.PACKAGE_IDS || '')
  .split(',')
  .map((id) => id.trim())
  .filter(Boolean);
const TARGET_STATUS = __ENV.TARGET_STATUS || 'PickedUp';
const ITERATIONS = Number(__ENV.ITERATIONS || PACKAGE_IDS.length || 1);
const VUS = Number(__ENV.UPDATE_VUS || Math.min(Math.max(PACKAGE_IDS.length, 1), 50));

export const options = {
  scenarios: {
    concurrent_status_updates: {
      executor: 'shared-iterations',
      vus: VUS,
      iterations: ITERATIONS,
      maxDuration: __ENV.MAX_DURATION || '1m',
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<750'],
    http_req_failed: ['rate<0.05'],
    checks: ['rate>0.95'],
  },
};

export function setup() {
  if (PACKAGE_IDS.length === 0) {
    fail('Set PACKAGE_IDS to comma-separated package ids before running stress_test.js.');
  }

  return {
    packageIds: PACKAGE_IDS,
    targetStatus: TARGET_STATUS,
  };
}

export default function (data) {
  const packageId = data.packageIds[__ITER % data.packageIds.length];
  const payload = JSON.stringify({
    status: data.targetStatus,
    location: __ENV.UPDATE_LOCATION || 'Stress test location',
    notes: __ENV.UPDATE_NOTES || 'Updated by k6 stress test',
    updatedBy: __ENV.UPDATED_BY || 'k6',
  });

  const response = http.post(`${BASE_URL}/api/packages/${packageId}/update`, payload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(response, {
    'status update returns 200': (res) => res.status === 200,
  });
}
