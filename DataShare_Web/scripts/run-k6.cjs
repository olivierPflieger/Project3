const fs = require('node:fs');
const path = require('node:path');
const { spawnSync } = require('node:child_process');

const repoRoot = path.resolve(__dirname, '..');
const defaultUploadFile = path.join(repoRoot, 'perf', 'fixtures', 'upload-test.txt');
const defaultBaseUrl = 'http://host.docker.internal:5051';
const defaultReportsDirectory = path.join(repoRoot, 'perf', 'reports');
const supportedProfiles = new Set(['smoke', 'load']);

function isInsideDirectory(parentPath, childPath) {
  const relativePath = path.relative(parentPath, childPath);
  return relativePath === '' || (!relativePath.startsWith('..') && !path.isAbsolute(relativePath));
}

function toContainerPathFromWorkspace(hostPath) {
  const relativePath = path.relative(repoRoot, hostPath).split(path.sep).join('/');
  return `/workspace/${relativePath}`;
}

function buildTimestamp(date) {
  const year = String(date.getFullYear());
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  const seconds = String(date.getSeconds()).padStart(2, '0');
  return `${year}${month}${day}-${hours}${minutes}${seconds}`;
}

const cliProfile = (process.argv[2] || '').trim().toLowerCase();
const profile = (process.env.K6_PROFILE || cliProfile || 'smoke').toLowerCase();

if (!supportedProfiles.has(profile)) {
  console.error(`Unsupported K6 profile "${profile}". Use one of: ${Array.from(supportedProfiles).join(', ')}.`);
  process.exit(1);
}

const requestedUploadFile = process.env.K6_UPLOAD_FILE
  ? path.resolve(process.env.K6_UPLOAD_FILE)
  : defaultUploadFile;

if (!fs.existsSync(requestedUploadFile)) {
  console.error(`K6 upload fixture was not found: ${requestedUploadFile}`);
  process.exit(1);
}

const reportTimestamp = buildTimestamp(new Date());
const requestedReportFile = process.env.K6_REPORT_FILE
  ? path.resolve(process.env.K6_REPORT_FILE)
  : path.join(defaultReportsDirectory, `k6-${profile}-${reportTimestamp}.html`);
const reportDirectory = path.dirname(requestedReportFile);

fs.mkdirSync(reportDirectory, { recursive: true });

const dockerArgs = [
  'run',
  '--rm',
  '-i',
  '-v',
  `${repoRoot}:/workspace`,
  '-w',
  '/workspace'
];

let containerUploadFile = toContainerPathFromWorkspace(requestedUploadFile);
let containerReportFile = toContainerPathFromWorkspace(requestedReportFile);

if (!isInsideDirectory(repoRoot, requestedUploadFile)) {
  const uploadDirectory = path.dirname(requestedUploadFile);
  dockerArgs.push('-v', `${uploadDirectory}:/k6-upload:ro`);
  containerUploadFile = `/k6-upload/${path.basename(requestedUploadFile)}`;
}

if (!isInsideDirectory(repoRoot, requestedReportFile)) {
  dockerArgs.push('-v', `${reportDirectory}:/k6-report`);
  containerReportFile = `/k6-report/${path.basename(requestedReportFile)}`;
}

const passthroughEnvNames = [
  'K6_BASE_URL',
  'K6_DURATION',
  'K6_VUS',
  'K6_ITERATIONS',
  'K6_FILE_PASSWORD',
  'K6_FILE_TAGS',
  'K6_FILE_EXPIRATION',
  'K6_UPLOAD_RESPONSE_P95',
  'K6_DOWNLOAD_RESPONSE_P95'
];

for (const envName of passthroughEnvNames) {
  if (process.env[envName]) {
    dockerArgs.push('-e', `${envName}=${process.env[envName]}`);
  }
}

dockerArgs.push(
  '-e',
  `K6_PROFILE=${profile}`,
  '-e',
  `K6_BASE_URL=${process.env.K6_BASE_URL || defaultBaseUrl}`,
  '-e',
  `K6_UPLOAD_FILE=${containerUploadFile}`,
  '-e',
  'K6_WEB_DASHBOARD=true',
  '-e',
  'K6_WEB_DASHBOARD_PORT=-1',
  '-e',
  `K6_WEB_DASHBOARD_EXPORT=${containerReportFile}`,
  'grafana/k6:latest',
  'run',
  '/workspace/perf/upload-download.js'
);

const result = spawnSync('docker', dockerArgs, {
  cwd: repoRoot,
  stdio: 'inherit'
});

if (result.error) {
  console.error(result.error.message);
  process.exit(1);
}

if (fs.existsSync(requestedReportFile)) {
  console.log(`K6 HTML report saved to ${requestedReportFile}`);
}

process.exit(result.status ?? 1);
