# Minimap renderer (`minimap`)

Aspire builds the local Python renderer from `wowskarma_api_minimap/Dockerfile.aspire`, starts the container, and checks its HTTP health endpoint at `/`. The API waits for the renderer and receives its Aspire HTTP endpoint as `MinimapApi__BaseUrl`.

The local renderer provisions the `karma-api` account at startup. Aspire generates and persists `Parameters:minimap-password` and `Parameters:minimap-signing-key`. It gives the password to the API and renderer and the token signing key to the renderer. Both keys accept explicit values. The renderer uses its bundled defaults of 15 frames per second, quality 5, and a 5 MiB replay upload limit. The API also limits replay uploads to 5 MiB.

## External renderer

Configure the following AppHost values to connect the API to an existing renderer. Use `Parameters:<name>` in AppHost configuration or `Parameters__<name>` in the environment. Enter the credential via the `aspire secret set "Parameters:minimap-password"` prompt or your environment's secret store.

| Parameter | Value |
| --- | --- |
| `minimap-base-url` | Base URL of the renderer, reachable from the API. Include a trailing `/` when the URL has a path prefix. Setting this selects the external renderer. |
| `minimap-login` | Username accepted by the remote renderer. Defaults to `karma-api`. |
| `minimap-password` | Required with the external renderer. Use the password of the configured renderer account. |

Aspire passes the external URL and credentials as `MinimapApi__BaseUrl`, `MinimapApi__Login`, and `MinimapApi__Password`. Configure rendering limits and token signing on the external renderer.
