# Deploy WOWS Karma

Deploy the ASP.NET Core API as a background service and serve the compiled Angular frontend over HTTPS. Each API process serves one World of Warships region and connects to PostgreSQL, Azure Blob Storage, and a minimap renderer. Use the [contributing guide](contributing.md) for the Aspire development stack. The AppHost runs the Angular development server and currently has no Aspire compute environment configured for a production deployment.

## Prepare an environment

- Choose the public web and API origins. Configure DNS and TLS for both. The Angular production configuration maps `wows-karma.com`, `na.wows-karma.com`, and `asia.wows-karma.com` to EU, NA, and SEA. The checked-in preview origin selects EU and sends browser API requests to `api.preview.wows-karma.com`. For a different domain or region layout, set `currentRegion` in the Angular environment for a single-region build or update the hostname mapping for a multi-region build. Check the matching API origins before building. See [Web configuration](aspire/web.md).
- Provision a persistent PostgreSQL database and role for each API region and environment. The API runs its data and authentication migrations on startup and Hangfire maintains its own schema. Give the application role the necessary schema and table creation permissions. Start one API instance to apply migrations before bringing up additional instances. See [PostgreSQL configuration](aspire/postgres.md).
- Provision replay storage for each environment. Create `replays` and `minimaps` with blob-level anonymous reads at an HTTPS endpoint reachable by browsers, since the API returns direct blob URLs. Keep `rce-replays` private. Use a connection string with the access needed to read, write, and delete blobs. See [Replay storage configuration](aspire/replay-storage.md).
- Provision a renderer that the API can reach and give it a dedicated service account. Keep the renderer on a private network where possible. For a self-managed renderer, use the image setup below.
- Obtain a Wargaming application ID for the chosen region. The OpenID flow must return through the browser to the public API endpoint at `/api/auth/wg-callback`.

### Run a self-managed renderer

Build the renderer from the repository root with Docker BuildKit. The build needs access to the registry, Python package index, and GitHub for the renderer source and assets.

```sh
docker build -f wowskarma_api_minimap/Dockerfile.aspire -t wowskarma-minimap wowskarma_api_minimap
```

The image listens on port `8080`. Provide its environment through your service manager or container runtime. The startup hook provisions the API account using `WOWSKARMA_MINIMAP_LOGIN` and `WOWSKARMA_MINIMAP_PASSWORD`. Supply a stable signing key as `WOWSKARMA_API_MINIMAP_SECURITY__SECRET_KEY`. For persistent user data, set `WOWSKARMA_API_MINIMAP_DB__URI` to a SQLAlchemy PostgreSQL URL and `WOWSKARMA_API_MINIMAP_DB__CONNECT_ARGS` to `@json {}`. Give the renderer database role permission to create its tables. See [Minimap configuration](aspire/minimap.md).

## Configure the API service

Inject these settings into the API process through the host's environment or secrets manager. Replace `<REGION>` with the selected region code, such as `EU`. Keep secrets out of the release artifacts and use distinct secrets in live and preview.

| Environment variable | Value |
| --- | --- |
| `DOTNET_ENVIRONMENT`, `ASPNETCORE_ENVIRONMENT` | `Production` for a production-style deployment, including preview. |
| `ASPNETCORE_URLS` | A private listener such as `http://127.0.0.1:5000` when the proxy is on the same host. Use a separate listener for each API process. |
| `Api__CurrentRegion` | The region served by this process. |
| `Api__<REGION>__AppId` | Wargaming application ID. |
| `Api__<REGION>__WgAuthCallback` | The full public HTTPS callback URL for this API. |
| `Api__<REGION>__CookieName` | The name used in the Angular build for this region, such as `Auth_EU`. |
| `Api__<REGION>__CookieDomain` | A parent domain shared by the web and API hosts, such as `wows-karma.com` for `wows-karma.com` and `api.wows-karma.com`. Omit it for a host-only cookie when both use the same hostname. |
| `JWT__Secret` | Stable random signing key of at least 32 bytes when encoded as UTF-8. Keep it consistent across replicas of the same regional API and across restarts to preserve active sessions. |
| `JWT__ValidIssuer`, `JWT__ValidAudience` | Stable identifiers for the regional API and its clients. Keep them consistent across replicas. |
| `ConnectionStrings__ApiDbConnectionString__<REGION>` | PostgreSQL connection string for the regional database. |
| `Api__<REGION>__Azure__Storage__ConnectionString` | Replay storage connection string. |
| `MinimapApi__BaseUrl`, `MinimapApi__Login`, `MinimapApi__Password` | Renderer base URL with a trailing slash and its service account credentials. |
| `Discord__WebhookAvatarPath` | Public image URL for webhook messages. The post and moderation services require it even when no webhook URLs are configured. |

Set `Database__PoolSize` to tune the EF Core context pools. The API uses 64 when this key is absent or zero. Use `Discord__Webhooks__<REGION>__Posts__0` and `Discord__Webhooks__<REGION>__ModActions__0` to configure notifications, with consecutive indices for additional webhooks. Use `AllowedProxies__0`, `AllowedProxies__1`, and so on to list the IPs of trusted reverse proxies when needed. Set `APPLICATIONINSIGHTS_CONNECTION_STRING` for API telemetry or `Aspire__DisableApplicationInsights=true` to skip Application Insights registration. Set `OTEL_EXPORTER_OTLP_ENDPOINT` to send logs, metrics, and traces to an OTLP collector. The [API configuration guide](aspire/api.md) covers corresponding AppHost settings for local development.

## Build and install the release

Use the .NET 10 SDK on the build machine and the ASP.NET Core 10 runtime on the API host. Build the frontend with Node.js 22 and pnpm. The Angular build fetches the Lato stylesheet from `fonts.googleapis.com` to inline its font definitions. Run these commands from the repository root.

```sh
dotnet publish WowsKarma.Api/WowsKarma.Api.csproj --configuration Release --no-self-contained -p:PublishReadyToRun=false --output WowsKarma.Api/bin/deploy
pnpm --dir wowskarma.app install
pnpm --dir wowskarma.app build --configuration production
```

For preview, build Angular with `--configuration preview`. The [production](../wowskarma.app/src/environments/environment.prod.ts) and [preview](../wowskarma.app/src/environments/environment.preview.ts) files supply API origins, cookie names, and browser Application Insights settings at build time. Any value in the browser bundle is public. Rebuild the frontend after changing these settings.

Copy the API publish directory to the API host. Run `dotnet WowsKarma.Api.dll` from the published directory under a dedicated service account. Use a process manager such as `systemd` to set the working directory, inject the service configuration, and restart the API. Ensure PostgreSQL, replay storage, and the renderer are reachable from the API host. The first successful API startup applies pending migrations before it begins listening. [ASP.NET Core hosting with Nginx and systemd](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-10.0)

Serve the contents of `wowskarma.app/dist/wowskarma.app/browser/` from the web document root. Configure a fallback to `index.html` for Angular routes so that direct links and reloads work. Deploy all generated files together, including the service worker manifest and hashed assets. [Angular deployment guide](https://angular.dev/tools/cli/deployment)

Route the public API hostname to the private API listener. Forward the `Host`, `X-Forwarded-For`, and `X-Forwarded-Proto` headers, and allow WebSocket upgrades on `/api/hubs/` for SignalR. Set the proxy request body limit above the 5 MiB replay limit to allow multipart form overhead. Enable HTTPS on the web and API origins. [ASP.NET Core reverse proxy configuration](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0)

## Release through GitHub Actions

The [Deploy to Server workflow](../.github/workflows/deploy.yml) is configured to publish preview on pushes to `develop` and `main`. The live job runs only on `main`. Manual dispatch follows the same live branch condition. Each deployment job downloads its API and web artifacts, copies them into `api/` and `web/` under the server deployment path, then runs the configured API restart command. Provision the runtime configuration and dependencies on each server independently of the workflow.

Configure the following GitHub Actions settings in each of the `preview` and `live` environments.

| Kind | Names and purpose |
| --- | --- |
| Secrets | `SSH_PRIVATEKEY`, `SSH_HOST`, `SSH_USER`, and `SSH_DEPLOYPATH` provide the SSH identity, server, user, and destination. The deployment account needs write access to that path. |
| Variable | `SSH_CMD_RESTART_API` is the server-side restart command. The SSH account needs permission to run it. |

The checked-in workflow still requests .NET 8 for solution and API packaging. Update its SDK inputs to .NET 10 before using it with the current `net10.0` projects. The root [Compose file](../docker-compose.yml) also predates the current stack. It references a web Dockerfile that is absent and an API Dockerfile based on .NET 6. Use the manual build and installation steps above for the current checkout. The separate [Minimap image workflow](../.github/workflows/minimap-api-push.yml) publishes `Dockerfile.dev`, which uses a different entry point from `Dockerfile.aspire`.

## Verify a release and recover from a failure

Retain the previous API and web artifacts and back up persistent stores before deploying.

1. Check API startup logs for the selected region, successful migrations, and the listening address. Request `GET /api/status` on the public API origin and expect HTTP 200. Request `GET /api/post/latest` to exercise database access.
2. Load the public web origin and reload a deep link. Sign in with a Wargaming account for the selected region and check that the browser returns through the public callback, receives the expected cookie, and connects to the SignalR hubs.
3. Upload a supported replay and verify that the replay download and rendered minimap video work from the browser. Check webhook delivery and telemetry when configured.

If verification fails, restore the previous artifacts together and restart the API if the database schema is compatible. Use the database backup or a migration-specific recovery plan when schema changes prevent an artifact-only rollback.
