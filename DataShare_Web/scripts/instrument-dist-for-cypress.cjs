const fs = require('node:fs');
const path = require('node:path');
const { createInstrumenter } = require('istanbul-lib-instrument');

const rootDir = path.resolve(__dirname, '..');
const outputDir = path.join(rootDir, '.cypress-coverage-dist');
const sourceDir = path.join(rootDir, 'src');

function resolveBuildDir() {
  const candidates = [
    path.join(rootDir, 'dist', 'frontend', 'browser'),
    path.join(rootDir, 'dist', 'browser'),
    path.join(rootDir, 'dist'),
  ];

  for (const candidate of candidates) {
    if (fs.existsSync(candidate) && fs.statSync(candidate).isDirectory()) {
      return candidate;
    }
  }

  throw new Error('Unable to locate the Angular build output directory in dist/.');
}

function shouldInstrument(filePath) {
  if (!filePath.endsWith('.js') || filePath.endsWith('.js.map')) {
    return false;
  }

  const fileName = path.basename(filePath);
  return !fileName.startsWith('polyfills') && !fileName.startsWith('runtime');
}

function instrumentDirectory(directoryPath, sourceRoot) {
  for (const entry of fs.readdirSync(directoryPath, { withFileTypes: true })) {
    const entryPath = path.join(directoryPath, entry.name);

    if (entry.isDirectory()) {
      instrumentDirectory(entryPath, sourceRoot);
      continue;
    }

    if (!shouldInstrument(entryPath)) {
      continue;
    }

    const sourceMapPath = `${entryPath}.map`;
    const inputCode = fs.readFileSync(entryPath, 'utf8');
    const inputSourceMap = fs.existsSync(sourceMapPath)
      ? JSON.parse(fs.readFileSync(sourceMapPath, 'utf8'))
      : undefined;

    const instrumenter = createInstrumenter({
      autoWrap: true,
      compact: false,
      esModules: true,
      produceSourceMap: Boolean(inputSourceMap),
    });

    const instrumentedCode = instrumenter.instrumentSync(inputCode, entryPath, inputSourceMap);
    fs.writeFileSync(entryPath, instrumentedCode);

    if (inputSourceMap) {
      fs.writeFileSync(sourceMapPath, JSON.stringify(instrumenter.lastSourceMap()));
    }
  }
}

const buildDir = resolveBuildDir();
fs.rmSync(outputDir, { recursive: true, force: true });
fs.cpSync(buildDir, outputDir, { recursive: true });
fs.cpSync(sourceDir, path.join(outputDir, 'src'), { recursive: true });
instrumentDirectory(outputDir, outputDir);
