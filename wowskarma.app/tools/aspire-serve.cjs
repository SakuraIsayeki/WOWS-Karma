const { spawn } = require('node:child_process');
const { writeFileSync } = require('node:fs');
const { resolve } = require('node:path');

const { PORT, WOWSKARMA_REGION = 'EU', WOWSKARMA_API_URL, WOWSKARMA_COOKIE_NAME, WOWSKARMA_COOKIE_DOMAIN,
  WOWSKARMA_WEB_CERT, WOWSKARMA_WEB_KEY, WOWSKARMA_WEB_APPLICATION_INSIGHTS } = process.env;

if (!PORT || !WOWSKARMA_API_URL || !WOWSKARMA_COOKIE_NAME || !WOWSKARMA_WEB_CERT || !WOWSKARMA_WEB_KEY) {
  throw new Error('Start the web app through the WOWS Karma Aspire AppHost.');
}

if (!['EU', 'NA', 'SEA'].includes(WOWSKARMA_REGION)) {
  throw new Error('WOWSKARMA_REGION must be EU, NA, or SEA.');
}

writeFileSync(resolve(__dirname, '../src/environments/environment.aspire.generated.ts'), `
import { environment as development } from './environment.development';
import { ApiRegion } from '../app/models/ApiRegion';

export const environment = {
  ...development,
  currentRegion: ApiRegion.${WOWSKARMA_REGION},
  apiHost: { ...development.apiHost, ${WOWSKARMA_REGION}: ${JSON.stringify(new URL(WOWSKARMA_API_URL).origin)} },
  cookies: {
    domain: ${JSON.stringify(WOWSKARMA_COOKIE_DOMAIN ?? '')},
    name: { ...development.cookies.name, ${WOWSKARMA_REGION}: ${JSON.stringify(WOWSKARMA_COOKIE_NAME)} },
  },
  applicationInsights: { connectionString: ${JSON.stringify(WOWSKARMA_WEB_APPLICATION_INSIGHTS ?? '')} },
};
`);

const angular = spawn(process.execPath, [require.resolve('@angular/cli/bin/ng.js'), 'serve',
  '--configuration', 'aspire', '--port', PORT, '--ssl',
  '--ssl-cert', WOWSKARMA_WEB_CERT, '--ssl-key', WOWSKARMA_WEB_KEY], { stdio: 'inherit' });

for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, () => angular.kill(signal));
}
angular.on('exit', (code) => { process.exitCode = code ?? 1; });
