using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var wargamingAppId = builder.AddParameter("wargaming-app-id", secret: true)
    .WithDescription("Wargaming Public API application ID for EU.");
var jwtSecret = Secret("jwt-secret", 48);
var jwtIssuer = builder.AddParameter("jwt-issuer", "localhost");
var jwtAudience = builder.AddParameter("jwt-audience", "localhost");
var cookieName = builder.AddParameter("auth-cookie-name", "Auth_EU");
var cookieDomain = builder.AddParameter("auth-cookie-domain", "localhost");
var databasePoolSize = builder.AddParameter("database-pool-size", "32");
var postgresPassword = Secret("postgres-password", 32);
var discordAvatar = builder.AddParameter("discord-webhook-avatar", "https://wows-karma.com/assets/media/logo.webp");

var database = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume()
    .AddDatabase("karma", "wowskarma_eu");

var seq = builder.AddSeq("seq").WithDataVolume();

var api = builder.AddDotnetProject("api", "../WowsKarma.Api/WowsKarma.Api.csproj", options => options.ExcludeLaunchProfile = true)
    .WithHttpsEndpoint()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/api/status", endpointName: "https")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Aspire")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Aspire")
    .WithEnvironment("Api__CurrentRegion", "EU")
    .WithEnvironment("Api__EU__AppId", wargamingAppId)
    .WithEnvironment("Api__EU__CookieName", cookieName)
    .WithEnvironment("Api__EU__CookieDomain", cookieDomain)
    .WithEnvironment("JWT__Secret", jwtSecret)
    .WithEnvironment("JWT__ValidIssuer", jwtIssuer)
    .WithEnvironment("JWT__ValidAudience", jwtAudience)
    .WithEnvironment("Database__PoolSize", databasePoolSize)
    .WithEnvironment("Discord__WebhookAvatarPath", discordAvatar)
    .WithReference(database, connectionName: "ApiDbConnectionString__EU")
    .WaitFor(database)
    .WithEnvironment("Aspire__SeqUrl", seq.GetEndpoint("http"))
    .WaitFor(seq);

var apiEndpoint = api.GetEndpoint("https");
if (OptionalParameter("wargaming-auth-callback") is { } callback)
    api.WithEnvironment("Api__EU__WgAuthCallback", callback);
else
    api.WithEnvironment("Api__EU__WgAuthCallback", $"{apiEndpoint}/api/auth/wg-callback");

if (OptionalParameter("replay-storage-connection-string", secret: true) is { } storageConnection)
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

var minimapLogin = builder.AddParameter("minimap-login", "karma-api");
var minimapPassword = Secret("minimap-password", 32);
api.WithEnvironment("MinimapApi__Login", minimapLogin)
    .WithEnvironment("MinimapApi__Password", minimapPassword);

if (OptionalParameter("minimap-base-url") is { } minimapUrl)
{
    api.WithEnvironment("MinimapApi__BaseUrl", minimapUrl);
}
else
{
    var minimapSigningKey = Secret("minimap-signing-key", 48);
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

if (OptionalParameter("api-application-insights", secret: true) is { } apiInsights)
    api.WithEnvironment("APPLICATIONINSIGHTS_CONNECTION_STRING", apiInsights);
else
    api.WithEnvironment("Aspire__DisableApplicationInsights", "true");

AddArrayParameters("discord-post-webhooks", "Discord__Webhooks__EU__Posts", secret: true);
AddArrayParameters("discord-mod-action-webhooks", "Discord__Webhooks__EU__ModActions", secret: true);
AddArrayParameters("allowed-proxies", "AllowedProxies", secret: false);

var web = builder.AddJavaScriptApp("web", "../wowskarma.app", runScriptName: "aspire")
    .WithPnpm()
    .WithHttpsEndpoint(env: "PORT");

#pragma warning disable ASPIRECERTIFICATES001 // Angular needs Aspire's HTTPS development key pair.
web.WithHttpsDeveloperCertificate()
    .WithHttpsCertificateConfiguration(context =>
    {
        context.EnvironmentVariables["WOWSKARMA_WEB_CERT"] = context.CertificatePath;
        context.EnvironmentVariables["WOWSKARMA_WEB_KEY"] = context.KeyPath;
        return Task.CompletedTask;
    });
#pragma warning restore ASPIRECERTIFICATES001

web.WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/", endpointName: "https")
    .WithEnvironment("WOWSKARMA_API_URL", apiEndpoint)
    .WithEnvironment("WOWSKARMA_COOKIE_NAME", cookieName)
    .WithEnvironment("WOWSKARMA_COOKIE_DOMAIN", cookieDomain)
    .WaitFor(api);

if (OptionalParameter("web-application-insights") is { } webInsights)
    web.WithEnvironment("WOWSKARMA_WEB_APPLICATION_INSIGHTS", webInsights);

builder.Build().Run();

IResourceBuilder<ParameterResource> Secret(string name, int length) =>
    builder.AddParameter(name, new GenerateParameterDefault { MinLength = length, Special = false },
        secret: true, persist: true);

IResourceBuilder<ParameterResource>? OptionalParameter(string name, bool secret = false)
{
    var parameter = builder.AddParameter(name, secret: secret).AsOptional();
    return string.IsNullOrWhiteSpace(builder.Configuration[$"Parameters:{name}"]) ? null : parameter;
}

void AddArrayParameters(string name, string environmentPrefix, bool secret)
{
    var values = builder.Configuration.GetSection($"Parameters:{name}").GetChildren();
    foreach (var (value, index) in values.Select((value, index) => (value, index)))
    {
        var parameter = builder.AddParameterFromConfiguration($"{name}-{index}", value.Path, secret);
        api.WithEnvironment($"{environmentPrefix}__{index}", parameter);
    }
}
