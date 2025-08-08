#!/usr/bin/env node

// Minimal golden tests to validate format and single-line output shape.

const assert = require('assert');
const { spawnSync } = require('child_process');

function run(cmd, args, input) {
  const res = spawnSync(cmd, args, { input, encoding: 'utf8' });
  return { code: res.status, stdout: (res.stdout || '').trim(), stderr: (res.stderr || '').trim() };
}

function mustHaveSingleLine(text) {
  const lines = text.split('\n').filter((l) => l.trim().length > 0);
  assert(lines.length === 1, 'output must be exactly one non-empty line');
}

function mustNotHaveFencesOrBackticks(text) {
  assert(!text.includes('```') && !text.includes('`'), 'no code fences/backticks allowed');
}

function runCase(prompt) {
  const { code, stdout } = run(process.execPath, ['../bin/termgen', '--model=cmdgen', prompt]);
  assert.strictEqual(code, 0, 'process should exit 0');
  mustHaveSingleLine(stdout);
  mustNotHaveFencesOrBackticks(stdout);
  console.log('OK:', prompt, '=>', stdout);
}

try {
  runCase('curl GET to http://example.com');
  runCase('curl POST JSON {"x":1} to http://localhost:3000/api');
  runCase('grep recursive for TODO in current dir');
  runCase('tar gzip folder ./project into project.tar.gz');
} catch (e) {
  console.error('Test failed:', e.message);
  process.exit(1);
}


