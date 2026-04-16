import http from 'k6/http';
import { check, fail } from 'k6';
import { Counter, Trend } from 'k6/metrics';

const baseUrl = __ENV.K6_BASE_URL || 'http://localhost:5051';
const profile = (__ENV.K6_PROFILE || 'smoke').toLowerCase();
const uploadFilePath = __ENV.K6_UPLOAD_FILE || '/workspace/perf/fixtures/upload-test.txt';
const useRandomUploadRange = (__ENV.K6_UPLOAD_RANDOM_RANGE || '').toLowerCase() === 'true';
const uploadMinMb = positiveInteger(__ENV.K6_UPLOAD_MIN_MB, 1);
const uploadMaxMb = positiveInteger(__ENV.K6_UPLOAD_MAX_MB, 100);
const uploadSourceMaxMb = positiveInteger(__ENV.K6_UPLOAD_SOURCE_MAX_MB, 100);
const filePassword = __ENV.K6_FILE_PASSWORD || 'k6-secret';
const fileTags = __ENV.K6_FILE_TAGS || 'k6,perf';
const fileExpiration = __ENV.K6_FILE_EXPIRATION || '1';
const uploadResponseP95 = __ENV.K6_UPLOAD_RESPONSE_P95 || '2000';
const downloadResponseP95 = __ENV.K6_DOWNLOAD_RESPONSE_P95 || '2000';
const summaryFilePath = __ENV.K6_SUMMARY_FILE || '';
const summaryJsonFilePath = __ENV.K6_SUMMARY_JSON_FILE || '';

const uploadRequestsTotal = new Counter('upload_requests_total');
const downloadRequestsTotal = new Counter('download_requests_total');
const uploadFailuresTotal = new Counter('upload_failures_total');
const downloadFailuresTotal = new Counter('download_failures_total');
const uploadSizeMiB = new Trend('upload_size_mib');
const uploadDurationAll = new Trend('upload_duration_custom');
const downloadDurationAll = new Trend('download_duration_custom');
const uploadDurationBucketSmall = new Trend('upload_duration_bucket_small');
const uploadDurationBucketMedium = new Trend('upload_duration_bucket_medium');
const uploadDurationBucketLarge = new Trend('upload_duration_bucket_large');
const downloadDurationBucketSmall = new Trend('download_duration_bucket_small');
const downloadDurationBucketMedium = new Trend('download_duration_bucket_medium');
const downloadDurationBucketLarge = new Trend('download_duration_bucket_large');
const uploadCountBucketSmall = new Counter('upload_count_bucket_small');
const uploadCountBucketMedium = new Counter('upload_count_bucket_medium');
const uploadCountBucketLarge = new Counter('upload_count_bucket_large');
const downloadCountBucketSmall = new Counter('download_count_bucket_small');
const downloadCountBucketMedium = new Counter('download_count_bucket_medium');
const downloadCountBucketLarge = new Counter('download_count_bucket_large');
const uploadFailuresBucketSmall = new Counter('upload_failures_bucket_small');
const uploadFailuresBucketMedium = new Counter('upload_failures_bucket_medium');
const uploadFailuresBucketLarge = new Counter('upload_failures_bucket_large');
const downloadFailuresBucketSmall = new Counter('download_failures_bucket_small');
const downloadFailuresBucketMedium = new Counter('download_failures_bucket_medium');
const downloadFailuresBucketLarge = new Counter('download_failures_bucket_large');

function positiveInteger(value, fallbackValue) {
  const parsedValue = Number.parseInt(value || '', 10);
  return Number.isFinite(parsedValue) && parsedValue > 0 ? parsedValue : fallbackValue;
}

function buildUploadFixtures() {
  if (!useRandomUploadRange) {
    const uploadFileName = uploadFilePath.split('/').pop() || 'upload-test.txt';

    return [
      {
        binary: open(uploadFilePath, 'b'),
        fileName: uploadFileName,
        contentType: 'text/plain',
        sizeMb: null
      }
    ];
  }

  const sourcePath = uploadFilePath || `/workspace/perf/fixtures/upload-test-${uploadSourceMaxMb}mb.bin`;

  return [
    {
      binary: open(sourcePath, 'b'),
      fileName: `upload-test-random-1-${uploadSourceMaxMb}mb.txt`,
      contentType: 'text/plain',
      sizeMb: null
    }
  ];
}

const uploadFixtures = buildUploadFixtures();

function buildOptions() {
  const loadVus = positiveInteger(__ENV.K6_SCENARIO_VUS, positiveInteger(__ENV.K6_VUS, 5));
  const loadDuration = __ENV.K6_SCENARIO_DURATION || __ENV.K6_DURATION || '1m';
  const smokeVus = positiveInteger(__ENV.K6_SCENARIO_VUS, positiveInteger(__ENV.K6_VUS, 1));
  const smokeIterations = positiveInteger(__ENV.K6_SCENARIO_ITERATIONS, positiveInteger(__ENV.K6_ITERATIONS, 1));
  const smokeMaxDuration = __ENV.K6_SCENARIO_DURATION || __ENV.K6_DURATION || '30s';
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
      summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
      thresholds,
      scenarios: {
        upload_download_load: {
          executor: 'constant-vus',
          vus: loadVus,
          duration: loadDuration
        }
      }
    };
  }

  return {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
    thresholds,
    scenarios: {
      upload_download_smoke: {
        executor: 'shared-iterations',
        vus: smokeVus,
        iterations: smokeIterations,
        maxDuration: smokeMaxDuration
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
    'register returns HTTP Status2xx': (response) => response.status >= 200 && response.status < 300
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

function randomIntInclusive(minValue, maxValue) {
  return Math.floor(Math.random() * (maxValue - minValue + 1)) + minValue;
}

function chooseUploadFixture() {
  const uploadFixture = uploadFixtures[0];

  if (!useRandomUploadRange) {
    return uploadFixture;
  }

  const minMb = Math.min(uploadMinMb, uploadMaxMb);
  const maxMb = Math.min(Math.max(uploadMinMb, uploadMaxMb), uploadSourceMaxMb);
  const selectedSizeMb = randomIntInclusive(minMb, maxMb);
  const selectedSizeBytes = selectedSizeMb * 1024 * 1024;

  return {
    binary: uploadFixture.binary.slice(0, selectedSizeBytes),
    fileName: `upload-test-${selectedSizeMb}mb.txt`,
    contentType: uploadFixture.contentType,
    sizeMb: selectedSizeMb
  };
}

function getSizeBucket(sizeMb) {
  if (sizeMb <= 10) {
    return 'small';
  }

  if (sizeMb <= 50) {
    return 'medium';
  }

  return 'large';
}

function recordUploadMetrics(sizeMb, durationMs, failed) {
  const bucket = getSizeBucket(sizeMb);

  uploadRequestsTotal.add(1);
  uploadSizeMiB.add(sizeMb);
  uploadDurationAll.add(durationMs);

  if (bucket === 'small') {
    uploadCountBucketSmall.add(1);
    uploadDurationBucketSmall.add(durationMs);
    if (failed) {
      uploadFailuresBucketSmall.add(1);
    }
    return;
  }

  if (bucket === 'medium') {
    uploadCountBucketMedium.add(1);
    uploadDurationBucketMedium.add(durationMs);
    if (failed) {
      uploadFailuresBucketMedium.add(1);
    }
    return;
  }

  uploadCountBucketLarge.add(1);
  uploadDurationBucketLarge.add(durationMs);
  if (failed) {
    uploadFailuresBucketLarge.add(1);
  }
}

function recordDownloadMetrics(sizeMb, durationMs, failed) {
  const bucket = getSizeBucket(sizeMb);

  downloadRequestsTotal.add(1);
  downloadDurationAll.add(durationMs);

  if (bucket === 'small') {
    downloadCountBucketSmall.add(1);
    downloadDurationBucketSmall.add(durationMs);
    if (failed) {
      downloadFailuresBucketSmall.add(1);
    }
    return;
  }

  if (bucket === 'medium') {
    downloadCountBucketMedium.add(1);
    downloadDurationBucketMedium.add(durationMs);
    if (failed) {
      downloadFailuresBucketMedium.add(1);
    }
    return;
  }

  downloadCountBucketLarge.add(1);
  downloadDurationBucketLarge.add(durationMs);
  if (failed) {
    downloadFailuresBucketLarge.add(1);
  }
}

function metricValues(data, metricName) {
  const metric = data.metrics[metricName];
  return metric ? metric.values : {};
}

function counterValue(data, metricName) {
  return metricValues(data, metricName).count || 0;
}

function trendValue(data, metricName, key) {
  const value = metricValues(data, metricName)[key];
  return Number.isFinite(value) ? value : null;
}

function percent(numerator, denominator) {
  if (!denominator) {
    return 0;
  }

  return (numerator / denominator) * 100;
}

function formatMs(value) {
  if (value === null) {
    return 'n/a';
  }

  if (value >= 1000) {
    return `${(value / 1000).toFixed(2)} s`;
  }

  return `${value.toFixed(0)} ms`;
}

function formatMiB(value) {
  if (value === null) {
    return 'n/a';
  }

  return `${value.toFixed(2)} MiB`;
}

function bucketSummary(data, label, uploadCountMetric, uploadDurationMetric, uploadFailureMetric, downloadCountMetric, downloadDurationMetric, downloadFailureMetric) {
  const uploadCount = counterValue(data, uploadCountMetric);
  const downloadCount = counterValue(data, downloadCountMetric);
  const uploadFailures = counterValue(data, uploadFailureMetric);
  const downloadFailures = counterValue(data, downloadFailureMetric);

  return {
    label,
    uploads: {
      count: uploadCount,
      failures: uploadFailures,
      failureRate: percent(uploadFailures, uploadCount),
      avg: trendValue(data, uploadDurationMetric, 'avg'),
      p90: trendValue(data, uploadDurationMetric, 'p(90)'),
      p95: trendValue(data, uploadDurationMetric, 'p(95)'),
      p99: trendValue(data, uploadDurationMetric, 'p(99)')
    },
    downloads: {
      count: downloadCount,
      failures: downloadFailures,
      failureRate: percent(downloadFailures, downloadCount),
      avg: trendValue(data, downloadDurationMetric, 'avg'),
      p90: trendValue(data, downloadDurationMetric, 'p(90)'),
      p95: trendValue(data, downloadDurationMetric, 'p(95)'),
      p99: trendValue(data, downloadDurationMetric, 'p(99)')
    }
  };
}

function buildSummaryModel(data) {
  const uploads = counterValue(data, 'upload_requests_total');
  const downloads = counterValue(data, 'download_requests_total');
  const uploadFailures = counterValue(data, 'upload_failures_total');
  const downloadFailures = counterValue(data, 'download_failures_total');

  return {
    scenario: profile === 'load' ? 'upload_download_load' : 'upload_download_smoke',
    vus: positiveInteger(__ENV.K6_SCENARIO_VUS, positiveInteger(__ENV.K6_VUS, profile === 'load' ? 5 : 1)),
    duration: __ENV.K6_SCENARIO_DURATION || __ENV.K6_DURATION || (profile === 'load' ? '1m' : '30s'),
    uploadRangeMiB: useRandomUploadRange ? `${Math.min(uploadMinMb, uploadMaxMb)}-${Math.min(Math.max(uploadMinMb, uploadMaxMb), uploadSourceMaxMb)}` : null,
    uploads: {
      count: uploads,
      failures: uploadFailures,
      failureRate: percent(uploadFailures, uploads),
      avg: trendValue(data, 'upload_duration_custom', 'avg'),
      p90: trendValue(data, 'upload_duration_custom', 'p(90)'),
      avgSizeMiB: trendValue(data, 'upload_size_mib', 'avg'),
      p95: trendValue(data, 'upload_duration_custom', 'p(95)'),
      p99: trendValue(data, 'upload_duration_custom', 'p(99)')
    },
    downloads: {
      count: downloads,
      failures: downloadFailures,
      failureRate: percent(downloadFailures, downloads),
      avg: trendValue(data, 'download_duration_custom', 'avg'),
      p90: trendValue(data, 'download_duration_custom', 'p(90)'),
      p95: trendValue(data, 'download_duration_custom', 'p(95)'),
      p99: trendValue(data, 'download_duration_custom', 'p(99)')
    },
    buckets: [
      bucketSummary(
        data,
        '1-10 MiB',
        'upload_count_bucket_small',
        'upload_duration_bucket_small',
        'upload_failures_bucket_small',
        'download_count_bucket_small',
        'download_duration_bucket_small',
        'download_failures_bucket_small'
      ),
      bucketSummary(
        data,
        '11-50 MiB',
        'upload_count_bucket_medium',
        'upload_duration_bucket_medium',
        'upload_failures_bucket_medium',
        'download_count_bucket_medium',
        'download_duration_bucket_medium',
        'download_failures_bucket_medium'
      ),
      bucketSummary(
        data,
        '51-100 MiB',
        'upload_count_bucket_large',
        'upload_duration_bucket_large',
        'upload_failures_bucket_large',
        'download_count_bucket_large',
        'download_duration_bucket_large',
        'download_failures_bucket_large'
      )
    ]
  };
}

function renderSummaryMarkdown(summary) {
  const lines = [
    '# k6 Campaign Summary',
    '',
    `- Scenario: ${summary.scenario}`,
    `- VUs: ${summary.vus}`,
    `- Duration: ${summary.duration}`
  ];

  if (summary.uploadRangeMiB) {
    lines.push(`- Random upload range: ${summary.uploadRangeMiB} MiB`);
  }

  lines.push(
    '',
    '## Global',
    '',
    `- Uploads: ${summary.uploads.count} (${summary.uploads.failures} failures, ${summary.uploads.failureRate.toFixed(2)}%), avg / p90 / p95 / p99: ${formatMs(summary.uploads.avg)} / ${formatMs(summary.uploads.p90)} / ${formatMs(summary.uploads.p95)} / ${formatMs(summary.uploads.p99)}`,
    `- Upload avg size: ${formatMiB(summary.uploads.avgSizeMiB)}`,
    `- Downloads: ${summary.downloads.count} (${summary.downloads.failures} failures, ${summary.downloads.failureRate.toFixed(2)}%), avg / p90 / p95 / p99: ${formatMs(summary.downloads.avg)} / ${formatMs(summary.downloads.p90)} / ${formatMs(summary.downloads.p95)} / ${formatMs(summary.downloads.p99)}`,
    '',
    '## By Size Bucket',
    ''
  );

  for (const bucket of summary.buckets) {
    lines.push(
      `### ${bucket.label}`,
      `- Uploads: ${bucket.uploads.count} (${bucket.uploads.failures} failures, ${bucket.uploads.failureRate.toFixed(2)}%), avg / p90 / p95 / p99: ${formatMs(bucket.uploads.avg)} / ${formatMs(bucket.uploads.p90)} / ${formatMs(bucket.uploads.p95)} / ${formatMs(bucket.uploads.p99)}`,
      `- Downloads: ${bucket.downloads.count} (${bucket.downloads.failures} failures, ${bucket.downloads.failureRate.toFixed(2)}%), avg / p90 / p95 / p99: ${formatMs(bucket.downloads.avg)} / ${formatMs(bucket.downloads.p90)} / ${formatMs(bucket.downloads.p95)} / ${formatMs(bucket.downloads.p99)}`,
      ''
    );
  }

  return lines.join('\n');
}

export function handleSummary(data) {
  const summary = buildSummaryModel(data);
  const markdown = renderSummaryMarkdown(summary);
  const outputs = {
    stdout: `${markdown}\n`
  };

  if (summaryFilePath) {
    outputs[summaryFilePath] = markdown;
  }

  if (summaryJsonFilePath) {
    outputs[summaryJsonFilePath] = JSON.stringify(summary, null, 2);
  }

  return outputs;
}

export default function runUploadDownloadScenario(setupData) {
  const uploadFixture = chooseUploadFixture();
  const operationTags = uploadFixture.sizeMb
    ? { operation: 'upload', upload_size_mb: String(uploadFixture.sizeMb) }
    : { operation: 'upload' };
  const uploadResponse = http.post(
    `${baseUrl}/api/files`,
    {
      password: filePassword,
      tags: fileTags,
      expiration: fileExpiration,
      file: http.file(uploadFixture.binary, uploadFixture.fileName, uploadFixture.contentType)
    },
    {
      headers: {
        Authorization: `Bearer ${setupData.token}`
      },
      tags: operationTags
    }
  );

  const uploadBody = uploadResponse.json();
  const uploadToken = uploadBody && uploadBody.token;

  const uploadOk = check(uploadResponse, {
    'upload returns HTTP Status 2xx': (response) => response.status >= 200 && response.status < 300,
    'upload returns a file token': () => Boolean(uploadToken)
  });

  recordUploadMetrics(uploadFixture.sizeMb || 0, uploadResponse.timings.duration, !uploadOk);

  if (!uploadOk) {
    uploadFailuresTotal.add(1);
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
      tags: uploadFixture.sizeMb
        ? { operation: 'download', upload_size_mb: String(uploadFixture.sizeMb) }
        : { operation: 'download' }
    }
  );

  const downloadOk = check(downloadResponse, {
    'download returns HTTP Status 200': (response) => response.status === 200,
    'download returns file content': (response) => response.body && response.body.byteLength > 0
  });

  recordDownloadMetrics(uploadFixture.sizeMb || 0, downloadResponse.timings.duration, !downloadOk);

  if (!downloadOk) {
    downloadFailuresTotal.add(1);
    fail(`Download failed: ${downloadResponse.status}`);
  }

  const deleteResponse = http.del(`${baseUrl}/api/files/${uploadToken}`, null, {
    headers: {
      Authorization: `Bearer ${setupData.token}`
    },
    tags: uploadFixture.sizeMb
      ? { operation: 'delete', upload_size_mb: String(uploadFixture.sizeMb) }
      : { operation: 'delete' }
  });

  const deleteOk = check(deleteResponse, {
    'delete returns HTTP Status 2xx': (response) => response.status >= 200 && response.status < 300
  });

  if (!deleteOk) {
    fail(`Delete failed: ${deleteResponse.status} ${deleteResponse.body}`);
  }
}
