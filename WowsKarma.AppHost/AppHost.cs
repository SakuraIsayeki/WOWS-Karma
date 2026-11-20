using WowsKarma.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    DashboardApplicationName = "WOWS Karma"
});

var wargamingAppId = builder.AddParameter("wargaming-app-id", secret: true)
    .WithDescription("Wargaming Public API application ID for EU.");
var jwtSecret = builder.AddGeneratedSecret("jwt-secret", 48);
var jwtIssuer = builder.AddParameter("jwt-issuer", "localhost");
var jwtAudience = builder.AddParameter("jwt-audience", "localhost");
var cookieName = builder.AddParameter("auth-cookie-name", "Auth_EU");
var cookieDomain = builder.AddParameter("auth-cookie-domain", "localhost");
var databasePoolSize = builder.AddParameter("database-pool-size", "32");
var postgresPassword = builder.AddGeneratedSecret("postgres-password", 32);
var discordAvatar = builder.AddParameter("discord-webhook-avatar", "https://wows-karma.com/assets/media/logo.webp");

var database = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume()
    .AddDatabase("karma", "wowskarma_eu");

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
    .WaitFor(database);

var apiEndpoint = api.GetEndpoint("https");
if (builder.AddOptionalParameter("wargaming-auth-callback") is { } callback)
    api.WithEnvironment("Api__EU__WgAuthCallback", callback);
else
    api.WithEnvironment("Api__EU__WgAuthCallback", $"{apiEndpoint}/api/auth/wg-callback");

api.WithReplayStorage()
    .WithMinimapApi()
    .WithArrayParameters("discord-post-webhooks", "Discord__Webhooks__EU__Posts", secret: true)
    .WithArrayParameters("discord-mod-action-webhooks", "Discord__Webhooks__EU__ModActions", secret: true)
    .WithArrayParameters("allowed-proxies", "AllowedProxies", secret: false);

if (builder.AddOptionalParameter("api-application-insights", secret: true) is { } apiInsights)
    api.WithEnvironment("APPLICATIONINSIGHTS_CONNECTION_STRING", apiInsights);
else
    api.WithEnvironment("Aspire__DisableApplicationInsights", "true");

var web = builder.AddJavaScriptApp("web", "../wowskarma.app", runScriptName: "aspire")
    .WithPnpm()
    .WithHttpsEndpoint(env: "PORT")
    .WithAngularDevelopmentCertificate()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/", endpointName: "https")
    .WithEnvironment("WOWSKARMA_API_URL", apiEndpoint)
    .WithEnvironment("WOWSKARMA_COOKIE_NAME", cookieName)
    .WithEnvironment("WOWSKARMA_COOKIE_DOMAIN", cookieDomain)
    .WaitFor(api);

if (builder.AddOptionalParameter("web-application-insights") is { } webInsights)
    web.WithEnvironment("WOWSKARMA_WEB_APPLICATION_INSIGHTS", webInsights);

builder.Build().Run();