import { readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { Window } from 'happy-dom';
import { PAGInit } from 'libpag';

const pagPath = process.argv[2];
if (!pagPath) {
  console.error('usage: node test-canvas2d.mjs <file.pag>');
  process.exit(1);
}

const window = new Window({ width: 1920, height: 1080 });
globalThis.window = window;
globalThis.document = window.document;
globalThis.HTMLCanvasElement = window.HTMLCanvasElement;
globalThis.OffscreenCanvas = window.OffscreenCanvas;
globalThis.ImageData = window.ImageData;
globalThis.getComputedStyle = window.getComputedStyle.bind(window);

const require = createRequire(import.meta.url);
const wasmPath = require.resolve('libpag/lib/libpag.wasm');
const wasmBytes = readFileSync(wasmPath);
const PAG = await PAGInit({
  wasmBinary: wasmBytes,
  locateFile: () => wasmPath,
});

const fileBuffer = readFileSync(pagPath);
const arrayBuffer = fileBuffer.buffer.slice(
  fileBuffer.byteOffset,
  fileBuffer.byteOffset + fileBuffer.byteLength,
);
const pagFile = await PAG.PAGFile.load(arrayBuffer);
const width = pagFile.width();
const height = pagFile.height();
console.log('loaded', width, height);

const canvas = document.createElement('canvas');
canvas.width = width;
canvas.height = height;

const pagView = await PAG.PAGView.init(pagFile, canvas, { firstFrame: true });
if (!pagView) {
  console.error('pagView init failed');
  process.exit(2);
}

pagView.setProgress(0.5);
await pagView.flush();

const ctx = canvas.getContext('2d');
const imageData = ctx.getImageData(0, 0, width, height);
console.log('pixels', imageData.data.length, 'sample', imageData.data.slice(0, 8));

pagView.destroy();
pagFile.destroy();
window.close();
