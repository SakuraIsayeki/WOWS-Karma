# Contributing to WOWS Karma

## Run the stack locally

Install [Git](https://git-scm.com/downloads), the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), [Aspire CLI 17](https://aspire.dev/get-started/install-cli/), [Node.js 22](https://nodejs.org/en/download), and [pnpm 12](https://pnpm.io/installation). Put the tools on your `PATH` and start [Docker or Podman](https://aspire.dev/get-started/prerequisites/). The minimap image contains Python and FFmpeg and uses Git `ADD`, which requires Docker with BuildKit enabled or Podman 5.3 or later.

Sign in to the [Wargaming Developer Room](https://developers.wargaming.net/applications/), add a development application, and copy its `application_id` for your selected region's API requests. Use a World of Warships account in that region to test OpenID sign-in. [Wargaming registration guide](https://developers.wargaming.net/documentation/guide/getting-started/)

Clone the repository, or use your fork’s URL if you plan to open a pull request.

```sh
git clone https://github.com/SakuraIsayeki/WOWS-Karma.git
cd WOWS-Karma
```

If you use Podman, select it in the shell where you will run Aspire. On Windows or macOS, start the Podman machine first if it is stopped.

| Shell | Select Podman |
| --- | --- |
| PowerShell | `$env:ASPIRE_CONTAINER_RUNTIME = "podman"` |
| Bash or Zsh | `export ASPIRE_CONTAINER_RUNTIME=podman` |

From the repository root, run the following commands. Enter the application ID at the secret prompt.

```sh
aspire certs trust
aspire secret set "Parameters:wargaming-app-id"
aspire run
```

Follow the dashboard URL in the terminal. Wait for `web` to become healthy, then open its HTTPS endpoint. Check the API at `/api/status` and open `/swagger` on the API’s HTTPS endpoint. A request to `/api/post/latest` exercises PostgreSQL and can return an empty list on a fresh database. Use the dashboard endpoints because Aspire assigns the ports.

The first run restores .NET and Angular dependencies, pulls infrastructure images, and builds the minimap renderer with its game assets. Each minimap image build resolves the latest commit on the renderer repository’s default branch. The API applies its data and authentication migrations and creates the Azurite containers before serving requests. PostgreSQL and Azurite persist their local data in container volumes. The API sends logs, metrics, and traces to the Aspire dashboard over OTLP. Press Ctrl+C in the terminal to stop the foreground run.

## Configure your local stack

The AppHost runs one region at a time and defaults to EU. Select `EU`, `NA`, or `SEA` with `Parameters:region` in AppHost configuration. For example, in PowerShell:

```powershell
$env:Parameters__region = "NA"
aspire run
```

Region values are case-insensitive. Unsupported values fail at startup. Restart Aspire after switching regions. The AppHost selects the API configuration sections, Wargaming callback, Discord webhook sections, replay storage settings, and Angular region, API URL, and cookie name. The dashboard shows the database as `api-db-<region>`, using the lowercase region code. Each region uses its own database in the PostgreSQL volume. Existing parameter overrides apply to the selected region. Update application IDs, external storage, callbacks, and webhook URLs as needed when switching.

Store overrides with `aspire secret set "Parameters:<name>"`, or supply them through AppHost configuration using `Parameters__<name>` environment variables. For an indexed value, use a key such as `Parameters:discord-post-webhooks:0` or an environment variable such as `Parameters__discord-post-webhooks__0`. Restart Aspire after changing settings. Keep credentials in local configuration and redact them from logs, screenshots, and pull requests.

See the service-specific configuration for the [API](aspire/api.md), [Angular web app](aspire/web.md), [PostgreSQL](aspire/postgres.md), [replay storage](aspire/replay-storage.md), and [minimap renderer](aspire/minimap.md). For preview and production, see the [deployment guide](deployment.md).

## Find your way around the code

| Directory | Start here for |
| --- | --- |
| [`WowsKarma.AppHost/`](../WowsKarma.AppHost/) | Local resources, parameters, startup order, and endpoints. |
| [`WowsKarma.Api/`](../WowsKarma.Api/) | ASP.NET Core API, Wargaming sign-in, data models and migrations, replay ingestion, and background jobs. |
| [`wowskarma.app/`](../wowskarma.app/) | Angular UI, routing, and browser API integration. |
| [`WowsKarma.Common/`](../WowsKarma.Common/) and [`WowsKarma.Api.Minimap.Client/`](../WowsKarma.Api.Minimap.Client/) | Shared API contracts and the .NET client for the minimap service. |
| [`wowskarma_api_minimap/`](../wowskarma_api_minimap/) | Python API and replay-to-video rendering. |
| [`WowsKarma.ServiceDefaults/`](../WowsKarma.ServiceDefaults/) | OpenTelemetry instrumentation and export setup. |

## Validate a change

For a .NET change, build the affected entry point from the root. Stop any running instance of the project before building if Windows reports locked output files.

```sh
dotnet build WowsKarma.Api/WowsKarma.Api.csproj
dotnet build WowsKarma.AppHost/WowsKarma.AppHost.csproj
```

For a web change, run the Angular development build from the root. Aspire normally installs dependencies on startup, so you can skip the install step if it has already completed and dependencies have stayed the same.

```sh
pnpm --dir wowskarma.app install
pnpm --dir wowskarma.app build --configuration development
```

For the Aspire frontend configuration checks, run `node --test wowskarma.app/tools/aspire-serve.test.cjs` from the root. These cover EU, NA, SEA, invalid regions, and hostname detection outside Aspire.

For changes to startup, authentication, storage, or replays, run the stack and exercise the affected flow through the dashboard endpoints. For replay changes, use a supported `.wowsreplay` fixture and check that the resulting minimap video plays. For the Python test and lint commands, see the [minimap contributor guide](../wowskarma_api_minimap/CONTRIBUTING.md). [Repository CI workflows](../.github/workflows/)

## Troubleshoot startup

| Symptom | What to check |
| --- | --- |
| Aspire cannot resolve the AppHost | Run from the repository root and check that `aspire ls` finds `WowsKarma.AppHost`. Use `aspire doctor` to check the local toolchain. |
| The dashboard waits for an installer or dependency | Open the earliest unhealthy resource’s console logs. The Angular installer needs `pnpm` on the same `PATH` as Aspire. The first minimap build needs registry, Python package index, and GitHub access. |
| Containers fail to start | Confirm that `docker info` or `podman info` can reach the engine from the same shell. On Podman, check `ASPIRE_CONTAINER_RUNTIME`. |
| Local HTTPS fails | Run `aspire certs trust` again and use the HTTPS URLs in the dashboard. |
| Wargaming requests or sign-in fail | Check that the application ID is set on the AppHost and works for the selected region. For sign-in, check the account's region, callback URL, and shared cookie domain. |
| PostgreSQL rejects the password | Check that the configured password matches the one used when the local volume was initialized. |

## Open a pull request

Create a topic branch from the intended target branch. Keep changes focused and include any relevant migration, configuration, or setup documentation. In the pull request, describe what changed, how you verified it, and any setup needed to reproduce the result.
