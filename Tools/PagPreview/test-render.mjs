import { readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import createGL from 'gl';
import { PAGInit } from 'libpag';

const pagPath = process.argv[2];
if (!pagPath) {
  console.error('usage: node test-render.mjs <file.pag>');
  process.exit(1);
}

const __dirname = dirname(fileURLToPath(import.meta.url));
const require = createRequire(import.meta.url);
const wasmPath = require.resolve('libpag/lib/libpag.wasm');

const PAG = await PAGInit({
  locateFile: () => wasmPath,
});

const buffer = readFileSync(pagPath);
const pagFile = await PAG.PAGFile.load(buffer);
const width = pagFile.width();
const height = pagFile.height();
console.log('size', width, height, 'duration', pagFile.duration());

const gl = createGL(width, height, { preserveDrawingBuffer: true });
const texture = gl.createTexture();
gl.bindTexture(gl.TEXTURE_2D, texture);
gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, width, height, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);

const pagSurface = PAG.PAGSurface.fromTexture(texture, width, height, false);
const player = PAG.PAGPlayer.create();
player.setSurface(pagSurface);
player.setComposition(pagFile);
player.setProgress(0.5);
await player.flush();

const pixels = pagSurface.readPixels(PAG.types.ColorType.BGRA_8888, PAG.types.AlphaType.Unpremultiplied);
console.log('pixels', pixels ? pixels.length : null);
player.destroy();
pagSurface.destroy();
pagFile.destroy();
gl.destroy();
