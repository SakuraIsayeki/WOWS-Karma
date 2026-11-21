const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { resolve } = require('node:path');
const { runInNewContext } = require('node:vm');
const ts = require('typescript');

const serve = readFileSync(resolve(__dirname, 'aspire-serve.cjs'), 'utf8');
const service = ts.transpileModule(readFileSync(resolve(__dirname,
  '../src/app/services/app-config.service.ts'), 'utf8'), {
  compilerOptions: { module: ts.ModuleKind.CommonJS, experimentalDecorators: true },
}).outputText;
const ApiRegion = { EU: 'EU', NA: 'NA', SEA: 'SEA', CIS: 'CIS' };

function appConfig(environment, host = 'localhost:12345') {
  const exports = {};
  runInNewContext(service, {
    exports, URL, window: { location: { host } },
    require: (name) => {
      if (name === '@angular/core') return { Injectable: () => (target) => target };
      if (name === 'package.json') return { default: { version: 'test' } };
      if (name.endsWith('/ApiRegion')) return { ApiRegion };
      if (name.endsWith('/environment')) return { environment };
      throw new Error(`Unexpected import: ${name}`);
    },
  });
  const config = new exports.AppConfigService();
  assert.equal(exports.AppConfigService.GetApiRegionFromLocation(), config.currentRegion);
  return config;
}

function generate(overrides = {}) {
  let generated;
  let spawned = false;
  const requireStub = (name) => {
    if (name === 'node:fs') return { writeFileSync: (_, content) => { generated = content; } };
    if (name === 'node:path') return { resolve };
    if (name === 'node:child_process') return {
      spawn: () => { spawned = true; return { on() {}, kill() {} }; },
    };
    throw new Error(`Unexpected import: ${name}`);
  };
  requireStub.resolve = () => 'ng.js';
  runInNewContext(serve, {
    __dirname, URL, require: requireStub,
    process: { execPath: 'node', on() {}, env: {
      PORT: '12345', WOWSKARMA_API_URL: 'https://localhost:54321/',
      WOWSKARMA_COOKIE_NAME: 'custom-cookie', WOWSKARMA_COOKIE_DOMAIN: 'localhost',
      WOWSKARMA_WEB_CERT: 'cert.pem', WOWSKARMA_WEB_KEY: 'key.pem', ...overrides,
    } },
  });
  const exports = {};
  runInNewContext(ts.transpileModule(generated, {
    compilerOptions: { module: ts.ModuleKind.CommonJS },
  }).outputText, {
    exports, require: (name) => name.endsWith('/ApiRegion') ? { ApiRegion } : {
      environment: {
        apiHost: Object.fromEntries(Object.values(ApiRegion).map(r => [r, 'https://original'])),
        cookies: { name: Object.fromEntries(Object.values(ApiRegion).map(r => [r, `Auth_${r}`])) },
      },
    },
  });
  assert.ok(spawned);
  return exports.environment;
}

for (const region of ['EU', 'NA', 'SEA']) {
  test(`${region}: generated settings drive Angular on localhost`, () => {
    const environment = generate({ WOWSKARMA_REGION: region });
    assert.equal(environment.currentRegion, region);
    assert.equal(environment.apiHost[region], 'https://localhost:54321');
    assert.equal(environment.cookies.name[region], 'custom-cookie');
    assert.equal(environment.cookies.domain, 'localhost');
    assert.equal(appConfig(environment).currentRegion, region);
    assert.equal(environment.apiHost.CIS, 'https://original');
    assert.equal(environment.applicationInsights.connectionString, '');
  });
}

test('missing Aspire region defaults to EU', () => {
  assert.equal(generate().currentRegion, 'EU');
});

test('unsupported Aspire regions fail before Angular starts', () => {
  for (const region of ['CIS', '', 'invalid']) {
    assert.throws(() => generate({ WOWSKARMA_REGION: region }), /must be EU, NA, or SEA/);
  }
});

test('ordinary frontend builds still infer region from hostname', () => {
  for (const [host, region] of [
    ['wows-karma.com', 'EU'], ['na.wows-karma.com', 'NA'],
    ['asia.wows-karma.com', 'SEA'], ['localhost:12345', 'EU'],
  ]) {
    assert.equal(appConfig({}, host).currentRegion, region);
  }
});
