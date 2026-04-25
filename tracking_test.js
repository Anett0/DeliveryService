import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m', target: 50 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.01'],
  },
};

const TRACKING_CODE = 'BR6N31W92UG2';

export default function () {
  const res = http.get(`http://localhost:5166/api/packages/${TRACKING_CODE}`);
  check(res, {
    'status is 200': (r) => r.status === 200,
    'response has trackingCode': (r) => r.json('trackingCode') === TRACKING_CODE,
  });
  sleep(1);
}