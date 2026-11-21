# Angular web app (`web`)

Aspire starts the Angular development server with the `aspire` pnpm script and waits for the API. Open the web HTTPS endpoint from the dashboard. Use `aspire certs trust` to trust the local HTTPS certificate before opening the site.

| AppHost parameter | Value |
| --- | --- |
| `region` | Shared with the API. The default is `EU`. `NA` and `SEA` are also supported. |
| `auth-cookie-domain` | Shared with the API. The default is the API HTTPS endpoint's host. Set it to a parent domain shared by the public API and web hosts if they use separate subdomains. |
| `web-application-insights` | Optional browser Application Insights connection string. Set `Parameters:web-application-insights` in AppHost configuration or `Parameters__web-application-insights` in the environment. The value is visible in the browser bundle. Browser telemetry is disabled by default. |

Aspire supplies the web port and certificate paths, the API HTTPS origin, and the regional cookie name (`Auth_EU`, `Auth_NA`, or `Auth_SEA`). The launcher writes these values and the shared region, cookie domain, and browser telemetry setting to `src/environments/environment.aspire.generated.ts` before running `ng serve --configuration aspire`. Restart Aspire after changing the AppHost inputs.

For the static Angular production build, see the [deployment guide](../deployment.md).
