const fs = require('node:fs');
const path = require('node:path');

const rootDir = path.resolve(__dirname, '..');
const targets = ['.nyc_output', 'coverage', '.cypress-coverage-dist'];

for (const target of targets) {
  fs.rmSync(path.join(rootDir, target), { recursive: true, force: true });
}
