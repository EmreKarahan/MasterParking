#!/usr/bin/env node
/*
 * One-off migration utility.
 * Extracts the embedded FREE_CAR_01 geometry + palette texture from the original
 * HTML prototype and writes standard OBJ/MTL/PNG assets for Unity.
 *
 * Each part is a separate OBJ so wheels can be rigged to pivot objects.
 * The source model remains untouched. Output is a clean, replaceable art asset.
 */
const fs = require('fs');
const path = require('path');

const SOURCE = process.argv[2] || 'C:/Users/Emre/Downloads/park_game (1).html';
const HTML = fs.readFileSync(SOURCE, 'utf8');
const OUT_MODELS = path.resolve(__dirname, '../../Assets/_Project/Art/Models/Vehicles');
const OUT_TEXTURES = path.resolve(__dirname, '../../Assets/_Project/Art/Textures');

const partsMatch = HTML.match(/const CAR_PARTS = (\[[\s\S]*?\]);\s*\r?\n\s*const CAR_PNG/);
if (!partsMatch) throw new Error('CAR_PARTS block not found');
const pngMatch = HTML.match(/const CAR_PNG = 'data:image\/png;base64,([A-Za-z0-9+/=]+)'/);
if (!pngMatch) throw new Error('CAR_PNG payload not found');

const parts = JSON.parse(partsMatch[1]);

function decode(str) { return Buffer.from(str, 'base64'); }
function floats(str) {
  const b = decode(str);
  const out = new Float32Array(b.length / 4);
  for (let i = 0; i < out.length; i++) out[i] = b.readFloatLE(i * 4);
  return out;
}
function int8s(str) {
  const b = decode(str);
  const out = new Int8Array(b.length);
  for (let i = 0; i < b.length; i++) out[i] = b.readInt8(i);
  return out;
}

const FILE_NAMES = {
  car_body: 'Car_Body.obj',
  door_r: 'Door_R.obj',
  door_l: 'Door_L.obj',
  win_door_r001: 'Window_Door_R.obj',
  win_door_l001: 'Window_Door_L.obj',
  windows: 'Windows.obj',
  interior: 'Interior.obj',
  number: 'Number.obj',
  wheel_fl: 'Wheel_FL.obj',
  wheel_fr: 'Wheel_FR.obj',
  wheel_bl: 'Wheel_BL.obj',
  wheel_br: 'Wheel_BR.obj'
};

function writePart(part) {
  const p = floats(part.p);
  const nr = int8s(part.nr);
  const uv = floats(part.u);
  const count = p.length / 3;
  if (nr.length !== count * 3 || uv.length !== count * 2) {
    throw new Error(`Mismatched attribute counts for ${part.n}`);
  }

  const lines = [
    `# ${part.n} extracted from FREE_CAR_01 (browser prototype) for Master Parking.`,
    '# +X = forward, +Y = up, +Z = right. One unit = one metre.',
    'mtllib CarPaint.mtl',
    `o ${part.n}`,
    `usemtl ${part.g ? 'CarGlass' : 'CarPaint'}`
  ];
  for (let i = 0; i < count; i++) {
    lines.push(`v ${p[i * 3].toFixed(6)} ${p[i * 3 + 1].toFixed(6)} ${p[i * 3 + 2].toFixed(6)}`);
  }
  for (let i = 0; i < count; i++) {
    lines.push(`vt ${uv[i * 2].toFixed(6)} ${uv[i * 2 + 1].toFixed(6)}`);
  }
  for (let i = 0; i < count; i++) {
    let x = nr[i * 3], y = nr[i * 3 + 1], z = nr[i * 3 + 2];
    const len = Math.hypot(x, y, z) || 1;
    lines.push(`vn ${(x / len).toFixed(6)} ${(y / len).toFixed(6)} ${(z / len).toFixed(6)}`);
  }
  for (let i = 0; i < count; i += 3) {
    const a = i + 1;
    lines.push(`f ${a}/${a}/${a} ${a + 1}/${a + 1}/${a + 1} ${a + 2}/${a + 2}/${a + 2}`);
  }

  const file = FILE_NAMES[part.n];
  fs.writeFileSync(path.join(OUT_MODELS, file), lines.join('\n') + '\n', 'utf8');
  return count / 3;
}

fs.mkdirSync(OUT_MODELS, { recursive: true });
fs.mkdirSync(OUT_TEXTURES, { recursive: true });

let tris = 0;
for (const part of parts) tris += writePart(part);

const mtl = [
  '# Shared by all extracted car parts. Texture is the original palette atlas.',
  'newmtl CarPaint',
  'Ka 1.000 1.000 1.000',
  'Kd 1.000 1.000 1.000',
  'Ks 0.080 0.080 0.080',
  'Ns 24.000',
  'd 1.0',
  'illum 2',
  'map_Kd ../Textures/VehiclePalette.png',
  '',
  'newmtl CarGlass',
  'Ka 1.000 1.000 1.000',
  'Kd 1.000 1.000 1.000',
  'Ks 0.450 0.450 0.450',
  'Ns 96.000',
  'd 1.0',
  'illum 2',
  'map_Kd ../Textures/VehiclePalette.png',
  ''
].join('\n');
fs.writeFileSync(path.join(OUT_MODELS, 'CarPaint.mtl'), mtl, 'utf8');
fs.writeFileSync(path.join(OUT_TEXTURES, 'VehiclePalette.png'), decode(pngMatch[1]));

const manifest = parts.map(part => ({
  Name: part.n,
  File: FILE_NAMES[part.n],
  Glass: !!part.g,
  Pivot: part.c ? { X: part.c[0], Y: part.c[1], Z: part.c[2] } : null
}));
fs.writeFileSync(path.join(OUT_MODELS, 'vehicle-parts.json'), JSON.stringify(manifest, null, 2) + '\n', 'utf8');

console.log(`Parts: ${parts.length}, triangles: ${tris}`);
console.log(`Model: ${OUT_MODELS}`);
console.log(`Palette: ${OUT_TEXTURES}/VehiclePalette.png (${decode(pngMatch[1]).length} bytes)`);
