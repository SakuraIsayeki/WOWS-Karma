using Nodsoft.Wargaming.Api.Common;
using Projects;
using WowsKarma.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

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
    });;

// API Service
var api = builder.AddProject<WowsKarma_Api>("api")
    .WithEnvironment("API__CurrentRegion", regionParam)
    .WithExternalHttpEndpoints();

foreach (var (region, db) in apiDbs)
{
    api.WithReference(db).WaitFor(db);
}

builder.Build().Run();
