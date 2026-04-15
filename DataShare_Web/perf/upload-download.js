import http from 'k6/http';
import { check, fail } from 'k6';

const baseUrl = __ENV.K6_BASE_URL || 'http://localhost:5051';
const profile = (__ENV.K6_PROFILE || 'smoke').toLowerCase();
const uploadFilePath = __ENV.K6_UPLOAD_FILE || '/workspace/perf/fixtures/upload-test.txt';
const filePassword = __ENV.K6_FILE_PASSWORD || 'k6-secret';
const fileTags = __ENV.K6_FILE_TAGS || 'k6,perf';
const fileExpiration = __ENV.K6_FILE_EXPIRATION || '1';
const uploadResponseP95 = __ENV.K6_UPLOAD_RESPONSE_P95 || '2000';
const downloadResponseP95 = __ENV.K6_DOWNLOAD_RESPONSE_P95 || '2000';
const uploadBinary = open(uploadFilePath, 'b');

function positiveInteger(value, fallbackValue) {
  const parsedValue = Number.parseInt(value || '', 10);
  return Number.isFinite(parsedValue) && parsedValue > 0 ? parsedValue : fallbackValue;
}

function buildOptions() {
  const thresholds = {
    checks: ['rate>0.99'],
    'http_req_failed{operation:register}': ['rate<0.01'],
    'http_req_failed{operation:login}': ['rate<0.01'],
    'http_req_failed{operation:upload}': ['rate<0.01'],
    'http_req_failed{operation:download}': ['rate<0.01'],
    'http_req_failed{operation:delete}': ['rate<0.01'],
    'http_req_duration{operation:upload}': [`p(95)<${uploadResponseP95}`],
    'http_req_duration{operation:download}': [`p(95)<${downloadResponseP95}`]
  };

  if (profile === 'load') {
    return {
      thresholds,
      scenarios: {
        upload_download_load: {
          executor: 'constant-vus',
          vus: positiveInteger(__ENV.K6_VUS, 5),
          duration: __ENV.K6_DURATION || '1m'
        }
      }
    };
  }

  return {
    thresholds,
    scenarios: {
      upload_download_smoke: {
        executor: 'shared-iterations',
        vus: positiveInteger(__ENV.K6_VUS, 1),
        iterations: positiveInteger(__ENV.K6_ITERATIONS, 1),
        maxDuration: __ENV.K6_DURATION || '30s'
      }
    }
  };
}

export const options = buildOptions();

function jsonParams(operation, token) {
  const headers = { 'Content-Type': 'application/json' };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return {
    headers,
    tags: { operation }
  };
}

function createUserCredentials() {
  const uniquePart = `${Date.now()}-${Math.floor(Math.random() * 1000000)}`;

  return {
    email: `k6-${uniquePart}@example.test`,
    password: `K6-${uniquePart}-Pass!`
  };
}

export function setup() {
  const credentials = createUserCredentials();

  const registerResponse = http.post(
    `${baseUrl}/api/users`,
    JSON.stringify(credentials),
    jsonParams('register')
  );

  const registerOk = check(registerResponse, {
    'register returns 2xx': (response) => response.status >= 200 && response.status < 300
  });

  if (!registerOk) {
    fail(`Unable to register K6 user: ${registerResponse.status} ${registerResponse.body}`);
  }

  const loginResponse = http.post(
    `${baseUrl}/api/login`,
    JSON.stringify(credentials),
    jsonParams('login')
  );

  const loginBody = loginResponse.json();
  const token = loginBody && loginBody.token;

  const loginOk = check(loginResponse, {
    'login returns 200': (response) => response.status === 200,
    'login returns a token': () => Boolean(token)
  });

  if (!loginOk) {
    fail(`Unable to login K6 user: ${loginResponse.status} ${loginResponse.body}`);
  }

  return {
    credentials,
    token
  };
}

export default function runUploadDownloadScenario(setupData) {
  const uploadResponse = http.post(
    `${baseUrl}/api/files`,
    {
      password: filePassword,
      tags: fileTags,
      expiration: fileExpiration,
      file: http.file(uploadBinary, 'upload-test.txt', 'text/plain')
    },
    {
      headers: {
        Authorization: `Bearer ${setupData.token}`
      },
      tags: { operation: 'upload' }
    }
  );

  const uploadBody = uploadResponse.json();
  const uploadToken = uploadBody && uploadBody.token;

  const uploadOk = check(uploadResponse, {
    'upload returns 2xx': (response) => response.status >= 200 && response.status < 300,
    'upload returns a file token': () => Boolean(uploadToken)
  });

  if (!uploadOk) {
    fail(`Upload failed: ${uploadResponse.status} ${uploadResponse.body}`);
  }

  const downloadResponse = http.post(
    `${baseUrl}/api/files/download/${uploadToken}`,
    JSON.stringify({ password: filePassword }),
    {
      headers: {
        Authorization: `Bearer ${setupData.token}`,
        'Content-Type': 'application/json'
      },
      responseType: 'binary',
      tags: { operation: 'download' }
    }
  );

  const downloadOk = check(downloadResponse, {
    'download returns 200': (response) => response.status === 200,
    'download returns file content': (response) => response.body && response.body.byteLength > 0
  });

  if (!downloadOk) {
    fail(`Download failed: ${downloadResponse.status}`);
  }

  const deleteResponse = http.del(`${baseUrl}/api/files/${uploadToken}`, null, {
    headers: {
      Authorization: `Bearer ${setupData.token}`
    },
    tags: { operation: 'delete' }
  });

  const deleteOk = check(deleteResponse, {
    'delete returns 2xx': (response) => response.status >= 200 && response.status < 300
  });

  if (!deleteOk) {
    fail(`Delete failed: ${deleteResponse.status} ${deleteResponse.body}`);
  }
}
