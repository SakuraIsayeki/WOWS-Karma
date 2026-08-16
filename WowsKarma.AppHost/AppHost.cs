using Nodsoft.Wargaming.Api.Common;
using Projects;
using WowsKarma.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
{
    Args = args, 
    DashboardApplicationName = "WOWS Karma"
});

#pragma warning disable ASPIREPERSISTENCE001
#pragma warning disable ASPIREINTERACTION001

// Database: Postgres
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPersistentLifetime();

var apiDbs = Enum.GetValuesAsUnderlyingType<Region>()
    .Cast<Region>()
    .Select(region => new KeyValuePair<Region, IResourceBuilder<PostgresDatabaseResource>>(region, postgres.AddDatabase($"api-db-{region}".ToLowerInvariant())))
    .ToDictionary();

// Parameters
var regionParam = builder.AddParameter("region")
    .WithCustomInput(_ => new()
    {
        Name = "API Region",
        Description = "The region this configuration is running in.",
        InputType = InputType.Choice,
        Options = AspireExtensions.GetEnumOptions<Region>()
    });

var wgAppId = builder.AddParameter("wg-app-id", secret: true)
    .WithCustomInput(_ => new()
    {
        Name = "Wargaming App ID",
        Description = "The Wargaming App ID to use for API requests.",
        InputType = InputType.SecretText
    });

var discordPostsWebhook = builder.AddParameter("discord-posts-webhook", secret: true)
    .WithCustomInput(_ => new()
    {
        Name = "Discord Posts Webhook",
        Description = "The Discord webhook URL to use for posting updates.",
        InputType = InputType.Text
    });

var discordModActionsWebhook = builder.AddParameter("discord-modactions-webhook", secret: true)
    .WithCustomInput(_ => new()
    {
        Name = "Discord Mod Actions Webhook",
        Description = "The Discord webhook URL to use for posting mod actions.",
        InputType = InputType.Text
    });

var minimapApiLogin = builder.AddParameter("minimap-api-login", secret: true)
    .WithCustomInput(_ => new()
    {
        Name = "Minimap API Login",
        Description = "The login to use for the Minimap API.",
        InputType = InputType.Text,
        Required = false
    });

var minimapApiPassword = builder.AddParameter("minimap-api-password", secret: true)
    .WithCustomInput(_ => new()
    {
        Name = "Minimap API Password",
        Description = "The password to use for the Minimap API.",
        InputType = InputType.SecretText,
        Required = false
    });


// API Service
var api = builder.AddProject<WowsKarma_Api>("api")
    .WithEnvironment("API__CurrentRegion", regionParam)
    .WithExternalHttpEndpoints();

var minimapApi = builder.AddExternalService("minimap-api", "https://minimap.api.wows-karma.com");
api.WithReference(minimapApi);
// api.WithEnvironment("MinimapApi__Login", minimapApiLogin);
// api.WithEnvironment("MinimapApi__Password", minimapApiPassword);

foreach (var (region, db) in apiDbs)
{
    api.WithReference(db, $"ApiDbConnectionString:{region:G}").WaitFor(db);
    api.WithEnvironment($"API__{region:G}__AppId", wgAppId);
    api.WithEnvironment($"Discord__Webhooks__{region:G}__Posts", discordPostsWebhook);
    api.WithEnvironment($"Discord__Webhooks__{region:G}__ModActions", discordModActionsWebhook);
}

builder.Build().Run();
