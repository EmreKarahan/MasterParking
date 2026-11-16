#!/usr/bin/env node
/*
 * One-off migration utility.
 * Reads the original browser prototype and extracts its 18 level definitions
 * into the engine-agnostic JSON consumed by the Unity project.
 *
 * The JSON uses PascalCase so it maps 1:1 onto the C# ScriptableObject DTOs
 * through Unity's JsonUtility without a second naming layer.
 *
 * Source of truth: C:\Users\Emre\Downloads\park_game (1).html
 * This script never writes to the source file.
 */
const fs = require('fs');
const path = require('path');
const vm = require('vm');

const SOURCE = process.argv[2] || 'C:/Users/Emre/Downloads/park_game (1).html';
const OUT = path.resolve(__dirname, '../../Assets/_Project/Resources/Levels/levels.json');

const html = fs.readFileSync(SOURCE, 'utf8');
const start = html.indexOf('const frame = (x1');
const end = html.indexOf('let activeLevel = null;');
if (start < 0 || end < 0) throw new Error('Level block not found in source HTML');

let block = html.slice(start, end);
block += '\n;globalThis.__result = { LEVELS, TIERS };';

const sandbox = { Math, PI: Math.PI };
vm.createContext(sandbox);
vm.runInContext(block, sandbox);
const { LEVELS, TIERS } = sandbox.__result;

const S = 0.1;              // three.js world scale: 100 physics units = 10 m
const FRAME_RATE = 60;      // original speed values are per frame
const round = (n) => Math.round(n * 10000) / 10000;

function point(p) { return { X: round(p[0] * S), Z: round(p[1] * S) }; }

function convertDecor(decor) {
  const d = {
    Paths: [], Lines: [], Zebras: [], Rects: [], Arrows: [],
    Signs: [], Lights: [], Lamps: [], Trees: [], Bumps: [], Chevrons: [], Gates: []
  };
  for (const p of (decor.paint || [])) {
    const kind = p[0];
    if (kind === 'path' || kind === 'line') {
      const entry = { Points: p[1].map(point) };
      (kind === 'path' ? d.Paths : d.Lines).push(entry);
    } else if (kind === 'zebra') {
      d.Zebras.push({ X: round(p[1] * S), Z: round(p[2] * S), Width: round(p[3] * S), Depth: round(p[4] * S), Axis: p[5] });
    } else if (kind === 'rect') {
      d.Rects.push({ X: round(p[1] * S), Z: round(p[2] * S), Width: round(p[3] * S), Depth: round(p[4] * S) });
    } else if (kind === 'arrow') {
      d.Arrows.push({ X: round(p[1] * S), Z: round(p[2] * S), Angle: round(p[3]) });
    }
  }
  for (const p of (decor.props || [])) {
    const kind = p[0];
    if (kind === 'sign') d.Signs.push({ Kind: p[1], X: round(p[2] * S), Z: round(p[3] * S), Angle: round(p[4]) });
    else if (kind === 'light') d.Lights.push({ X: round(p[1] * S), Z: round(p[2] * S), Angle: round(p[3]), State: p[4] || 'g' });
    else if (kind === 'stop') d.Signs.push({ Kind: 'stop', X: round(p[1] * S), Z: round(p[2] * S), Angle: round(p[3]) });
    else if (kind === 'lamp') d.Lamps.push({ X: round(p[2] * S), Z: round(p[3] * S), Angle: round(p[4] || 0) });
    else if (kind === 'tree') d.Trees.push({ X: round(p[1] * S), Z: round(p[2] * S), Scale: round(p[3] || 1) });
    else if (kind === 'bump') d.Bumps.push({ X: round(p[1] * S), Z: round(p[2] * S), Length: round(p[3] * S), Angle: round(p[4]) });
    else if (kind === 'chev') d.Chevrons.push({ X: round(p[1] * S), Z: round(p[2] * S), Angle: round(p[3]) });
    else if (kind === 'gate') d.Gates.push({ X: round(p[1] * S), Z: round(p[2] * S), Angle: round(p[3]) });
  }
  return d;
}

const levels = LEVELS.map((lv, i) => {
  const tierIndex = i < 4 ? 0 : i < 8 ? 1 : i < 12 ? 2 : i < 16 ? 3 : 4;
  const tier = TIERS[tierIndex];
  return {
    Id: i + 1,
    Name: lv.name,
    ParTime: lv.parTime,
    Difficulty: tierIndex + 1,
    Start: { X: round(lv.start.x * S), Z: round(lv.start.y * S), Angle: round(lv.start.angle) },
    Target: {
      X: round(lv.target.x * S),
      Z: round(lv.target.y * S),
      Angle: round(lv.target.angle),
      Width: round(lv.target.w * S),
      Depth: round(lv.target.h * S),
      ToleranceDistance: round(lv.target.tolD * S),
      ToleranceAngle: round(lv.target.tolA)
    },
    Walls: lv.walls.map(w => ({ X: round(w.x * S), Z: round(w.y * S), Width: round(w.w * S), Depth: round(w.h * S) })),
    ParkedCars: lv.parkedCars.map(c => ({ X: round(c.x * S), Z: round(c.y * S), Angle: round(c.angle), Color: c.color })),
    Cones: lv.cones.map(c => ({ X: round(c.x * S), Z: round(c.y * S), Radius: round(c.r * S) })),
    Movers: lv.movers.map(m => ({
      X: round(m.x * S), Z: round(m.y * S),
      DirectionX: round(m.vx * FRAME_RATE * S),
      DirectionZ: round(m.vy * FRAME_RATE * S),
      MinX: round(m.minX * S), MaxX: round(m.maxX * S),
      MinZ: round(m.minY * S), MaxZ: round(m.maxY * S),
      Color: m.color
    })),
    Decor: convertDecor(lv.decor || { paint: [], props: [] })
  };
});

const output = {
  Version: 1,
  Source: 'park_game (1).html',
  WorldScale: S,
  Levels: levels
};

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(output, null, 2) + '\n', 'utf8');
console.log(`Wrote ${levels.length} levels to ${OUT}`);
console.log(levels.map(l => `${l.Id}. ${l.Name} (par ${l.ParTime}s, tier ${l.Difficulty})`).join('\n'));
