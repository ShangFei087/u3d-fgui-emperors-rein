import { createServer } from 'node:http';
import { readFileSync, existsSync } from 'node:fs';
import { extname, join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import puppeteer from 'puppeteer';

const __dirname = dirname(fileURLToPath(import.meta.url));
const PORT = Number(process.env.PAG_PREVIEW_PORT || 17420);
const libDir = join(__dirname, 'node_modules', 'libpag', 'lib');
const pagJs = readFileSync(join(libDir, 'libpag.min.js'));
const pagWasm = readFileSync(join(libDir, 'libpag.wasm'));

let browser;
let page;
let currentPath = '';
let currentPagBytes = null;
let pagMeta = { width: 0, height: 0, duration: 0 };

async function ensureBrowser() {
  if (browser) {
    return;
  }
  browser = await puppeteer.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-dev-shm-usage'],
  });
  page = await browser.newPage();
  await page.goto(`http://127.0.0.1:${PORT}/preview.html`, { waitUntil: 'networkidle0' });
}

async function loadPag(path) {
  await ensureBrowser();
  if (!existsSync(path)) {
    throw new Error(`pag not found: ${path}`);
  }
  currentPagBytes = readFileSync(path);
  currentPath = path;
  pagMeta = await page.evaluate(async () => {
    const resp = await fetch('/api/pagdata');
    const buffer = await resp.arrayBuffer();
    return window.__pagPreview.load(buffer);
  });
  return pagMeta;
}

async function renderFrame(progress) {
  if (!currentPath) {
    throw new Error('pag not loaded');
  }
  const pngBase64 = await page.evaluate(async (p) => window.__pagPreview.render(p), progress);
  return Buffer.from(pngBase64, 'base64');
}

function contentType(filePath) {
  switch (extname(filePath)) {
    case '.js':
      return 'application/javascript; charset=utf-8';
    case '.wasm':
      return 'application/wasm';
    case '.html':
      return 'text/html; charset=utf-8';
    default:
      return 'application/octet-stream';
  }
}

const previewHtml = `<!DOCTYPE html>
<html>
<head><meta charset="utf-8"><title>PAG Preview</title></head>
<body>
<script src="/libpag.min.js"></script>
<script>
window.__pagPreview = {
  pagView: null,
  pagFile: null,
  async load(buffer) {
    if (this.pagView) {
      await this.pagView.destroy();
      this.pagView = null;
    }
    if (this.pagFile) {
      this.pagFile.destroy();
      this.pagFile = null;
    }
    const PAG = await window.libpag.PAGInit({
      locateFile: () => '/libpag.wasm'
    });
    this.pagFile = await PAG.PAGFile.load(buffer);
    const canvas = document.createElement('canvas');
    canvas.width = this.pagFile.width();
    canvas.height = this.pagFile.height();
    document.body.appendChild(canvas);
    this.pagView = await PAG.PAGView.init(this.pagFile, canvas, { firstFrame: true });
    return {
      width: this.pagFile.width(),
      height: this.pagFile.height(),
      duration: this.pagFile.duration()
    };
  },
  async render(progress) {
    if (!this.pagView) throw new Error('pag not loaded');
    this.pagView.setProgress(progress);
    await this.pagView.flush();
    const canvas = this.pagView.canvasElement || document.querySelector('canvas');
    const dataUrl = canvas.toDataURL('image/png');
    return dataUrl.substring(dataUrl.indexOf(',') + 1);
  }
};
</script>
</body>
</html>`;

const server = createServer(async (req, res) => {
  try {
    const url = new URL(req.url, `http://127.0.0.1:${PORT}`);
    if (url.pathname === '/preview.html') {
      res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
      res.end(previewHtml);
      return;
    }
    if (url.pathname === '/libpag.min.js') {
      res.writeHead(200, { 'Content-Type': contentType('.js') });
      res.end(pagJs);
      return;
    }
    if (url.pathname === '/libpag.wasm') {
      res.writeHead(200, { 'Content-Type': contentType('.wasm') });
      res.end(pagWasm);
      return;
    }
    if (url.pathname === '/api/pagdata') {
      if (!currentPagBytes) {
        res.writeHead(404);
        res.end('pag not loaded');
        return;
      }
      res.writeHead(200, { 'Content-Type': 'application/octet-stream' });
      res.end(currentPagBytes);
      return;
    }
    if (url.pathname === '/api/load' && req.method === 'POST') {
      let body = '';
      for await (const chunk of req) body += chunk;
      const payload = JSON.parse(body || '{}');
      const meta = await loadPag(payload.path);
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ ok: true, meta }));
      return;
    }
    if (url.pathname === '/api/frame') {
      const progress = Number(url.searchParams.get('progress') || '0');
      const png = await renderFrame(progress);
      res.writeHead(200, { 'Content-Type': 'image/png' });
      res.end(png);
      return;
    }
    if (url.pathname === '/health') {
      res.writeHead(200, { 'Content-Type': 'text/plain' });
      res.end('ok');
      return;
    }
    res.writeHead(404);
    res.end('not found');
  } catch (error) {
    res.writeHead(500, { 'Content-Type': 'text/plain; charset=utf-8' });
    res.end(String(error?.stack || error));
  }
});

server.listen(PORT, '127.0.0.1', () => {
  console.log(`PAG preview server listening on http://127.0.0.1:${PORT}`);
});

process.on('SIGINT', async () => {
  if (browser) await browser.close();
  process.exit(0);
});
