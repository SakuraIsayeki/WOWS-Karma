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
        /// Credentials are configured in both modes. When <c>Parameters:minimap-base-url</c> is nonblank,
        /// the external service must accept those credentials. Otherwise, the local Dockerfile resource
        /// receives the same credentials, a generated signing key, and configurable rendering limits;
        /// the API uses its HTTP endpoint and waits for it before starting.
        /// </remarks>
        public IResourceBuilder<DotnetProjectResource> WithMinimapApi()
        {
            var builder = api.ApplicationBuilder;

            var minimapLogin = builder.AddParameter("minimap-login", "karma-api");
            var minimapPassword = builder.AddGeneratedSecret("minimap-password", 32);
            api.WithEnvironment("MinimapApi__Login", minimapLogin)
                .WithEnvironment("MinimapApi__Password", minimapPassword);

            if (builder.AddOptionalParameter("minimap-base-url") is { } minimapUrl)
            {
                api.WithEnvironment("MinimapApi__BaseUrl", minimapUrl);
            }
            else
            {
                var minimapSigningKey = builder.AddGeneratedSecret("minimap-signing-key", 48);
                var minimapFps = builder.AddParameter("minimap-fps", "15");
                var minimapQuality = builder.AddParameter("minimap-quality", "5");
                var minimapMaxFileSize = builder.AddParameter("minimap-max-file-size", "5242880");

                var minimap = builder.AddDockerfile("minimap", "../wowskarma_api_minimap", "Dockerfile.aspire")
                    .WithHttpEndpoint(targetPort: 8080)
                    .WithHttpHealthCheck("/")
                    .WithEnvironment("WOWSKARMA_API_MINIMAP_SECURITY__SECRET_KEY", minimapSigningKey)
                    .WithEnvironment("WOWSKARMA_API_MINIMAP_REPLAY__FPS", minimapFps)
                    .WithEnvironment("WOWSKARMA_API_MINIMAP_REPLAY__QUALITY", minimapQuality)
                    .WithEnvironment("WOWSKARMA_API_MINIMAP_REPLAY__MAX_FILE_SIZE", minimapMaxFileSize)
                    .WithEnvironment("WOWSKARMA_MINIMAP_LOGIN", minimapLogin)
                    .WithEnvironment("WOWSKARMA_MINIMAP_PASSWORD", minimapPassword);

                api.WithEnvironment("MinimapApi__BaseUrl", minimap.GetEndpoint("http")).WaitFor(minimap);
            }

            return api;
        }
    }
}
