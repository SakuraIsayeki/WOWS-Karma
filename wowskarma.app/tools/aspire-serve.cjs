const { spawn } = require('node:child_process');
const { writeFileSync } = require('node:fs');
const { resolve } = require('node:path');

const { PORT, WOWSKARMA_API_URL, WOWSKARMA_COOKIE_NAME, WOWSKARMA_COOKIE_DOMAIN,
  WOWSKARMA_WEB_CERT, WOWSKARMA_WEB_KEY, WOWSKARMA_WEB_APPLICATION_INSIGHTS } = process.env;

if (!PORT || !WOWSKARMA_API_URL || !WOWSKARMA_COOKIE_NAME || !WOWSKARMA_WEB_CERT || !WOWSKARMA_WEB_KEY) {
  throw new Error('Start the web app through the WOWS Karma Aspire AppHost.');
}

writeFileSync(resolve(__dirname, '../src/environments/environment.aspire.generated.ts'), `
import { environment as development } from './environment.development';

export const environment = {
  ...development,
  apiHost: { ...development.apiHost, EU: ${JSON.stringify(new URL(WOWSKARMA_API_URL).origin)} },
  cookies: {
    domain: ${JSON.stringify(WOWSKARMA_COOKIE_DOMAIN ?? '')},
    name: { ...development.cookies.name, EU: ${JSON.stringify(WOWSKARMA_COOKIE_NAME)} },
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
