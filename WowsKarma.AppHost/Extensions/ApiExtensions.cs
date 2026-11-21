using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Dotnet;

namespace WowsKarma.AppHost.Extensions;

/// <summary>
/// Configures the API's replay storage and minimap renderer, using external services when supplied
/// and local Aspire-managed resources otherwise.
/// </summary>
internal static class ApiExtensions
{
    extension(IResourceBuilder<DotnetProjectResource> api)
    {
        /// <summary>
        /// Connects the API to replay blob storage for the selected region.
        /// </summary>
        /// <param name="region">The validated region code used in the API's configuration keys (EU, NA, or SEA).</param>
        /// <returns>The API resource builder for further configuration.</returns>
        /// <remarks>
        /// A nonblank <c>Parameters:replay-storage-connection-string</c> selects external storage.
        /// Otherwise, a persistent Azurite emulator is added, container initialization is enabled,
        /// and the API waits for blob storage before starting. External storage is not initialized here.
        /// </remarks>
        public IResourceBuilder<DotnetProjectResource> WithReplayStorage(string region)
        {
            var builder = api.ApplicationBuilder;

            if (builder.AddOptionalParameter("replay-storage-connection-string", secret: true) is { } storageConnection)
            {
                api.WithEnvironment($"Api__{region}__Azure__Storage__ConnectionString", storageConnection);
            }
            else
            {
                var blobs = builder.AddAzureStorage("storage")
                    .RunAsEmulator(azurite => azurite.WithDataVolume())
                    .AddBlobs("replay-storage");

                api.WithEnvironment($"Api__{region}__Azure__Storage__ConnectionString", blobs.Resource.ConnectionStringExpression)
                    .WithEnvironment($"Api__{region}__Azure__Storage__InitializeContainers", "true")
                    .WaitFor(blobs);
            }

            return api;
        }

        /// <summary>
        /// Configures minimap client credentials and selects an external renderer or a local container.
        /// </summary>
        /// <returns>The API resource builder for further configuration.</returns>
        /// <remarks>
        /// When <c>Parameters:minimap-base-url</c> is supplied, the API uses credentials for that service.
        /// The local Dockerfile resource uses a fixed login and generated secrets. The API uses its HTTP
        /// endpoint and waits for it before starting.
        /// </remarks>
        public IResourceBuilder<DotnetProjectResource> WithMinimapApi()
        {
            var builder = api.ApplicationBuilder;

            const string defaultLogin = "karma-api";

            if (builder.AddOptionalParameter("minimap-base-url") is { } minimapUrl)
            {
                var minimapPassword = builder.AddParameter("minimap-password", secret: true);

                api.WithEnvironment("MinimapApi__BaseUrl", minimapUrl)
                    .WithEnvironment("MinimapApi__Password", minimapPassword);

                if (builder.AddOptionalParameter("minimap-login") is { } minimapLogin)
                    api.WithEnvironment("MinimapApi__Login", minimapLogin);
                else
                    api.WithEnvironment("MinimapApi__Login", defaultLogin);
            }
            else
            {
                var minimapSigningKey = builder.CreateGeneratedSecret("minimap-signing-key", 48);
                var minimapPassword = builder.CreateGeneratedSecret("minimap-password", 32);

                var minimap = builder.AddDockerfile("minimap", "../wowskarma_api_minimap", "Dockerfile.aspire")
                    .WithHttpEndpoint(targetPort: 8080)
                    .WithHttpHealthCheck("/")
                    .WithEnvironment("WOWSKARMA_API_MINIMAP_SECURITY__SECRET_KEY", minimapSigningKey)
                    .WithEnvironment("WOWSKARMA_MINIMAP_LOGIN", defaultLogin)
                    .WithEnvironment("WOWSKARMA_MINIMAP_PASSWORD", minimapPassword);

                api.WithEnvironment("MinimapApi__BaseUrl", minimap.GetEndpoint("http"))
                    .WithEnvironment("MinimapApi__Login", defaultLogin)
                    .WithEnvironment("MinimapApi__Password", minimapPassword)
                    .WaitFor(minimap);
            }

            return api;
        }
    }
}
