const fs = require('node:fs');
const http = require('node:http');
const path = require('node:path');
const { URL } = require('node:url');

const port = Number(process.env.COVERAGE_PORT || 4201);
const backendOrigin = process.env.COVERAGE_API_TARGET || 'http://localhost:5051';
const backendUrl = new URL(backendOrigin);
const staticRoot = path.resolve(__dirname, '..', '.cypress-coverage-dist');
const indexPath = path.join(staticRoot, 'index.html');

if (!fs.existsSync(indexPath)) {
  throw new Error(`Coverage index file not found at ${indexPath}`);
}

const contentTypes = new Map([
  ['.css', 'text/css; charset=utf-8'],
  ['.html', 'text/html; charset=utf-8'],
  ['.ico', 'image/x-icon'],
  ['.js', 'application/javascript; charset=utf-8'],
  ['.json', 'application/json; charset=utf-8'],
  ['.map', 'application/json; charset=utf-8'],
  ['.png', 'image/png'],
  ['.svg', 'image/svg+xml'],
  ['.txt', 'text/plain; charset=utf-8'],
]);

function sendFile(res, filePath) {
  const extension = path.extname(filePath).toLowerCase();
  const contentType = contentTypes.get(extension) || 'application/octet-stream';
  res.writeHead(200, {
    'Cache-Control': 'no-store',
    'Content-Type': contentType,
  });
  fs.createReadStream(filePath).pipe(res);
}

function proxyApi(req, res) {
  const proxyRequest = http.request(
    {
      hostname: backendUrl.hostname,
      port: backendUrl.port,
      path: req.url,
      method: req.method,
      headers: {
        ...req.headers,
        host: backendUrl.host,
      },
    },
    (proxyResponse) => {
      res.writeHead(proxyResponse.statusCode || 502, proxyResponse.headers);
      proxyResponse.pipe(res);
    },
  );

  proxyRequest.on('error', (error) => {
    res.writeHead(502, { 'Content-Type': 'text/plain; charset=utf-8' });
    res.end(`Coverage proxy error: ${error.message}`);
  });

  req.pipe(proxyRequest);
}

const server = http.createServer((req, res) => {
  const requestUrl = new URL(req.url || '/', `http://${req.headers.host || 'localhost'}`);
  const pathname = decodeURIComponent(requestUrl.pathname);

  if (pathname.startsWith('/api')) {
    proxyApi(req, res);
    return;
  }

  const requestedPath = pathname === '/' ? '/index.html' : pathname;
  const filePath = path.join(staticRoot, requestedPath);
  const normalizedFilePath = path.normalize(filePath);

  if (!normalizedFilePath.startsWith(staticRoot)) {
    res.writeHead(403, { 'Content-Type': 'text/plain; charset=utf-8' });
    res.end('Forbidden');
    return;
  }

  if (fs.existsSync(normalizedFilePath) && fs.statSync(normalizedFilePath).isFile()) {
    sendFile(res, normalizedFilePath);
    return;
  }

  sendFile(res, indexPath);
});

server.listen(port, () => {
  console.log(`Coverage server listening on http://localhost:${port}`);
  console.log(`Proxying /api to ${backendOrigin}`);
});
