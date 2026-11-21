# Replay storage (`storage`, `replay-storage`)

The default is an Aspire-managed Azurite container with a persistent data volume. Aspire passes the blob endpoint and connection string to the API and waits for the emulator. At startup, the API creates the blob containers if they are missing.

To use an external Azure Blob Storage account, supply its connection string through `aspire secret set "Parameters:replay-storage-connection-string"` or `Parameters__replay-storage-connection-string`. Aspire passes that connection string to `Api__<region>__Azure__Storage__ConnectionString` and skips the emulator. Provision the following containers before running the API.

| Container | Access | Contents |
| --- | --- | --- |
| `replays` | Anonymous blob reads | Public replay files. |
| `minimaps` | Anonymous blob reads | Rendered minimap videos. |
| `rce-replays` | Private | Quarantined replays. |

The API must be able to write blobs to all three containers. The replay download and minimap video URLs are direct blob URLs, so browsers need access to the public storage endpoint. Use a browser-reachable HTTPS blob endpoint when serving the web app over HTTPS in production.
