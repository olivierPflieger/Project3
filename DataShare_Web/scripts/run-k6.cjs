const fs = require('node:fs');
const path = require('node:path');
const { spawnSync } = require('node:child_process');

const repoRoot = path.resolve(__dirname, '..');
const defaultUploadFile = path.join(repoRoot, 'perf', 'fixtures', 'upload-test.txt');
const defaultBaseUrl = 'http://host.docker.internal:5051';
const supportedProfiles = new Set(['smoke', 'load']);

function isInsideDirectory(parentPath, childPath) {
  const relativePath = path.relative(parentPath, childPath);
  return relativePath === '' || (!relativePath.startsWith('..') && !path.isAbsolute(relativePath));
}

function toContainerPathFromWorkspace(hostPath) {
  const relativePath = path.relative(repoRoot, hostPath).split(path.sep).join('/');
  return `/workspace/${relativePath}`;
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

if (!isInsideDirectory(repoRoot, requestedUploadFile)) {
  const uploadDirectory = path.dirname(requestedUploadFile);
  dockerArgs.push('-v', `${uploadDirectory}:/k6-upload:ro`);
  containerUploadFile = `/k6-upload/${path.basename(requestedUploadFile)}`;
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

process.exit(result.status ?? 1);
