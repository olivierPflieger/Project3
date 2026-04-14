const fs = require('node:fs');
const path = require('node:path');

const rootDir = path.resolve(__dirname, '..');
const sourceFile = path.join(rootDir, 'coverage', 'unit', 'coverage-final.json');
const tempDir = path.join(rootDir, '.nyc_output');
const targetFile = path.join(tempDir, 'jest-unit.json');

if (!fs.existsSync(sourceFile)) {
  console.warn(`Jest coverage file not found at ${sourceFile}. Skipping merge input copy.`);
  process.exit(0);
}

fs.mkdirSync(tempDir, { recursive: true });
fs.copyFileSync(sourceFile, targetFile);
