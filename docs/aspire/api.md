# API (`api`)

The AppHost runs one region at a time. Set `Parameters:region` to `EU`, `NA`, or `SEA`. The default is `EU`, and values are case-insensitive. The region selects the Wargaming API, PostgreSQL database, Discord webhook configuration, and Angular settings. Restart Aspire after changing it and update the external values for the new region.

## AppHost inputs

Set values in AppHost configuration as `Parameters:<name>` or through environment variables as `Parameters__<name>`. For secrets, use the prompt from `aspire secret set "Parameters:<name>"`.

| Parameter | Value |
| --- | --- |
| `wargaming-app-id` | Required. Wargaming Public API application ID for the selected region. |
| `wargaming-auth-callback` | Optional full HTTPS URL to `/api/auth/wg-callback` when the browser-facing API address differs from Aspire's API HTTPS endpoint. |
| `auth-cookie-domain` | Optional shared cookie domain for the API and web hosts, for example `example.com` for `api.example.com` and `app.example.com`. The default is the Aspire API HTTPS endpoint's host. |
| `jwt-secret` | Aspire generates and persists the signing key. Supply an external key of at least 32 bytes if deployments must share a signing key or retain existing sessions. Replacing the key invalidates existing tokens. |
| `discord-post-webhooks:0` | Optional Discord incoming webhook URL for post notifications in the selected region. |
| `discord-mod-action-webhooks:0` | Optional Discord incoming webhook URL for moderation notifications in the selected region. |
| `allowed-proxies:0` | Optional IP address of a trusted reverse proxy sending `X-Forwarded-For` and `X-Forwarded-Proto`. Use an IP address such as `192.0.2.10`. |
| `api-application-insights` | Optional Application Insights connection string. Without it, the AppHost disables the API's Application Insights integration. The Aspire dashboard still receives API telemetry over OTLP. |

Use an index for every webhook or proxy entry, starting at `0`. For example, the second post webhook can be set with `aspire secret set "Parameters:discord-post-webhooks:1"` or the `Parameters__discord-post-webhooks__1` environment variable. Restart Aspire after changing settings.

## Managed configuration

The AppHost sets `Api__CurrentRegion`, supplies the PostgreSQL connection from `api-db-<region>` using a lowercase region in the resource name, and runs the API in the `Aspire` environment. The API and Angular share the cookie name `Auth_<region>` and cookie domain. The Wargaming callback defaults to `/api/auth/wg-callback` at the Aspire API HTTPS endpoint. JWT issuer and audience are `localhost`, the EF Core context pool size is `32` per context, and Discord webhooks use the WOWS Karma logo as their avatar.

The API applies its data and authentication migrations at startup. Use its dashboard HTTPS endpoint to check `/api/status` and open `/swagger`. See [PostgreSQL](postgres.md), [replay storage](replay-storage.md), and the [minimap renderer](minimap.md) for dependencies.
