using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Dotnet;

namespace WowsKarma.AppHost.Extensions;

internal static class ApiExtensions
{
    extension(IResourceBuilder<DotnetProjectResource> api)
    {
        public IResourceBuilder<DotnetProjectResource> WithReplayStorage()
        {
            var builder = api.ApplicationBuilder;

            if (builder.AddOptionalParameter("replay-storage-connection-string", secret: true) is { } storageConnection)
            {
                api.WithEnvironment("Api__EU__Azure__Storage__ConnectionString", storageConnection);
            }
            else
            {
                var blobs = builder.AddAzureStorage("storage")
                    .RunAsEmulator(azurite => azurite.WithDataVolume())
                    .AddBlobs("replay-storage");

                api.WithEnvironment("Api__EU__Azure__Storage__ConnectionString", blobs.Resource.ConnectionStringExpression)
                    .WithEnvironment("Api__EU__Azure__Storage__InitializeContainers", "true")
                    .WaitFor(blobs);
            }

            return api;
        }

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
