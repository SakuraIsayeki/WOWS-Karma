using WowsKarma.AppHost.Extensions;

// Compose one region's database, API, replay services, and Angular development server.
// Parameters are read through Aspire configuration (including user secrets and environment variables).
var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    DashboardApplicationName = "WOWS Karma"
});

// Normalize once: the region determines both configuration keys and the physical database name.
var region = (builder.Configuration["Parameters:region"] ?? "EU").Trim().ToUpperInvariant();
if (region is not ("EU" or "NA" or "SEA"))
    throw new ArgumentException("Parameters:region must be EU, NA, or SEA.");

// The Wargaming application ID must be supplied. Generated JWT and database secrets are persisted
// for reuse; the remaining parameters provide overridable defaults for local development.
var wargamingAppId = builder.AddParameter("wargaming-app-id", secret: true)
    .WithDescription($"Wargaming Public API application ID for {region}.");
var jwtSecret = builder.AddGeneratedSecret("jwt-secret", 48);
var jwtIssuer = builder.AddParameter("jwt-issuer", "localhost");
var jwtAudience = builder.AddParameter("jwt-audience", "localhost");
var cookieName = builder.AddParameter("auth-cookie-name", $"Auth_{region}");
var cookieDomain = builder.AddParameter("auth-cookie-domain", "localhost");
var databasePoolSize = builder.AddParameter("database-pool-size", "32");
var postgresPassword = builder.AddGeneratedSecret("postgres-password", 32);
var discordAvatar = builder.AddParameter("discord-webhook-avatar", "https://wows-karma.com/assets/media/logo.webp");

// Retain PostgreSQL data in a volume and isolate this region's data in its own database.
var database = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume()
    .AddDatabase("karma", $"wowskarma_{region.ToLowerInvariant()}");

// Aspire owns endpoint allocation rather than the project's launch profile. The Aspire environment
// selects the API's orchestration-specific settings; double underscores map to nested configuration.
// The regional connection name matches the API's connection-string lookup, and WaitFor gates startup.
var api = builder.AddDotnetProject("api", "../WowsKarma.Api/WowsKarma.Api.csproj", options => options.ExcludeLaunchProfile = true)
    .WithHttpsEndpoint()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/api/status", endpointName: "https")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Aspire")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Aspire")
    .WithEnvironment("Api__CurrentRegion", region)
    .WithEnvironment($"Api__{region}__AppId", wargamingAppId)
    .WithEnvironment($"Api__{region}__CookieName", cookieName)
    .WithEnvironment($"Api__{region}__CookieDomain", cookieDomain)
    .WithEnvironment("JWT__Secret", jwtSecret)
    .WithEnvironment("JWT__ValidIssuer", jwtIssuer)
    .WithEnvironment("JWT__ValidAudience", jwtAudience)
    .WithEnvironment("Database__PoolSize", databasePoolSize)
    .WithEnvironment("Discord__WebhookAvatarPath", discordAvatar)
    .WithReference(database, connectionName: $"ApiDbConnectionString__{region}")
    .WaitFor(database);

// Keep the endpoint as an Aspire reference so the callback follows the allocated HTTPS address.
// An explicit callback parameter supports setups that need a different externally reachable URL.
var apiEndpoint = api.GetEndpoint("https");
if (builder.AddOptionalParameter("wargaming-auth-callback") is { } callback)
    api.WithEnvironment($"Api__{region}__WgAuthCallback", callback);
else
    api.WithEnvironment($"Api__{region}__WgAuthCallback", $"{apiEndpoint}/api/auth/wg-callback");

// Optional external services replace the local storage/renderer defaults. Array parameters become
// indexed environment keys so the API can bind webhook and proxy lists through .NET configuration.
api.WithReplayStorage(region)
    .WithMinimapApi()
    .WithArrayParameters("discord-post-webhooks", $"Discord__Webhooks__{region}__Posts", secret: true)
    .WithArrayParameters("discord-mod-action-webhooks", $"Discord__Webhooks__{region}__ModActions", secret: true)
    .WithArrayParameters("allowed-proxies", "AllowedProxies", secret: false);

// Opt in to API Application Insights only with a supplied connection string. Otherwise, explicitly
// disable its registration in Startup; Aspire's own telemetry remains configured independently.
if (builder.AddOptionalParameter("api-application-insights", secret: true) is { } apiInsights)
    api.WithEnvironment("APPLICATIONINSIGHTS_CONNECTION_STRING", apiInsights);
else
    api.WithEnvironment("Aspire__DisableApplicationInsights", "true");

// The pnpm aspire script launches Angular with Aspire's port and certificate paths. Sharing the API
// endpoint and cookie settings keeps the frontend's API requests and authentication aligned.
var web = builder.AddJavaScriptApp("web", "../wowskarma.app", runScriptName: "aspire")
    .WithPnpm()
    .WithHttpsEndpoint(env: "PORT")
    .WithAngularDevelopmentCertificate()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/", endpointName: "https")
    .WithEnvironment("WOWSKARMA_REGION", region)
    .WithEnvironment("WOWSKARMA_API_URL", apiEndpoint)
    .WithEnvironment("WOWSKARMA_COOKIE_NAME", cookieName)
    .WithEnvironment("WOWSKARMA_COOKIE_DOMAIN", cookieDomain)
    .WaitFor(api);

// Browser telemetry is separately optional; its connection string is passed to the web launcher.
if (builder.AddOptionalParameter("web-application-insights") is { } webInsights)
    web.WithEnvironment("WOWSKARMA_WEB_APPLICATION_INSIGHTS", webInsights);

builder.Build().Run();
