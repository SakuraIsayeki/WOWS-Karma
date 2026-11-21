# PostgreSQL (`postgres`)

Aspire shows the database as `api-db-eu`, `api-db-na`, or `api-db-sea` according to the [selected region](api.md). These resources use the PostgreSQL databases `wowskarma_eu`, `wowskarma_na`, and `wowskarma_sea`, respectively. The API waits for the database and receives its connection string as `ConnectionStrings__ApiDbConnectionString__<REGION>`, with the region in uppercase. Its application data, authentication data, and Hangfire jobs use that regional database.

Aspire generates and persists the `postgres-password` secret and uses a persistent container volume. An existing volume retains the password used when PostgreSQL was initialized. When reusing that volume with a different AppHost secret store, restore the matching value using the `aspire secret set "Parameters:postgres-password"` prompt or `Parameters__postgres-password`. Keep the password and the volume together when moving an environment. Switching regions selects a different database in the same volume.

For operating PostgreSQL outside the AppHost, see the [deployment guide](../deployment.md).
