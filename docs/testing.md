# API end-to-end baseline

`tests/WowsKarma.Api.E2E` uses `TUnit.Aspire` to manage its dedicated AppHost. The
AppHost starts the actual API executable, PostgreSQL, and Azurite. API startup
applies both sets of EF migrations and creates the blob containers. Aspire
allocates the ports and container names for each test session. [TUnit Aspire integration](https://tunit.dev/docs/examples/aspire/)

Existing application defects are retained as failing tests, as requested. Read
[the findings report](../.github/report-api-e2e-baseline.md) before treating the
suite as a passing refactor gate. Production code has not been changed to make
the tests pass.

## Run

Install the .NET 10 SDK and run Docker or Podman with a working container engine.
The first run downloads the PostgreSQL and Azurite images. For Podman, set
`ASPIRE_CONTAINER_RUNTIME=podman` in the shell before running the commands.

```shell
dotnet build tests/WowsKarma.Api.E2E/WowsKarma.Api.E2E.csproj
dotnet run --project tests/WowsKarma.Api.E2E --no-build -- --report-trx --results-directory tests/TestResults
```

TUnit runs on Microsoft.Testing.Platform. The executable command above works
without changing the solution's test runner selection or using a VSTest adapter.
Use `-- --help` to see filtering and reporting options. A failed test exits nonzero.

## Measure API coverage

Build the test project in Debug as shown above, then run these commands from the
repository root. Aspire launches the prebuilt API DLL so a second build does not
replace the instrumented assembly. The coverage tool restores the DLL after the
run. [Microsoft's coverage tool documentation](https://learn.microsoft.com/en-us/dotnet/core/additional-tools/dotnet-coverage)

```shell
dotnet tool restore
dotnet tool run dotnet-coverage collect --settings tests/api-coverage.xml --include-files "$PWD/WowsKarma.Api/bin/Debug/net10.0/WowsKarma.Api.dll" --output-format cobertura --output tests/TestResults/api-coverage.cobertura.xml "dotnet run --project tests/WowsKarma.Api.E2E --no-build -- --report-trx --results-directory tests/TestResults"
pwsh -File tests/Measure-ApiCoverage.ps1
```

Coverage counts executable lines and branches in the running API. The settings
exclude EF migrations and the API assembly copy loaded by the test runner for
fixture setup. The summary checks for executed controller lines before reporting
overall coverage and a line breakdown by controller. The CI job publishes this
summary and uploads the raw Cobertura report alongside the test results.

See the measured baseline in [the findings report](../.github/report-api-e2e-baseline.md#api-code-coverage).

## Test layout

`Http/` groups scenarios by API area. `Http/Mod/` has separate classes for post
moderation actions and platform bans. `Http/Infrastructure/` covers ETags and CORS.
`Realtime/` groups SignalR scenarios by hub. `Support/` contains the shared Aspire
fixture, request builders, and response checks.

Each test has a short PascalCase method name and a descriptive TUnit display name.
Test classes have categories that can be used with `--treenode-filter`, for example
`--treenode-filter "/*/*/*/*[Category=Players]"` to select the player tests.

## Isolation and assertions

The `AspireFixture` starts one application per test session, waits
for the API health check, and shuts down the resources after the suite. The API
fixture adds database seeding, authenticated clients, and writes the captured
`api.log` to `tests/TestResults` at teardown. Each scenario creates its own
accounts and records so tests can run concurrently. Setup and selected persistence
checks access the database directly. API operations and DTO readback use HTTP or
SignalR. JWTs use a test-only signing key and real seed records, exercising the
production validators.
The test host uses its own working directory and configuration instead of the
API's local settings files and user secrets.

Assertions cover response codes, selected DTO fields, creation locations,
pagination, persisted changes, karma effects, and realtime events. Missing replay
metadata currently returns 204; post deletion returns 205. These are baseline
contracts, even where controller documentation differs. When intentionally
changing behavior, update the corresponding assertion and describe the contract
change in the refactor's review.

## Coverage boundaries

The suite covers status/OpenAPI, cached players and clans, karma queries, post
creation/edit/deletion, ownership and moderation locks, profile flags and cooldowns,
JWT/cookie validation, refresh/revocation, replay metadata and upload authorization,
admin job access, moderation actions, platform bans, and SignalR connections,
notification ownership, and seed invalidation events. The baseline runs in EU.

The test-only `WowsKarma.Api.TestOverrides` assembly is injected into the disposable
API with .NET startup hooks and ASP.NET Core hosting startup. It installs an
outbound HTTP handler that supplies explicit Wargaming search/OpenID fixtures and
missing-resource responses. Unhandled Wargaming requests fail rather than reaching
the live provider. It does not replace controllers, business services, JWT
validation, EF contexts, or blob clients. It is not referenced by production
projects. The tests do not verify the live provider's behavior or availability.

Successful binary replay ingestion and rendering require a redistributable representative
replay sample and renderer fixture. Current replay metadata tests seed parsed
records; they do not claim to verify binary parsing, blob upload, or rendering.
Admin job acceptance tests verify HTTP acceptance, not worker completion. Extend
these boundaries as fixtures are added; do not replace them with tests against
live Wargaming accounts.
