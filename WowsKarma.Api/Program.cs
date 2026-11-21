using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WowsKarma.Api;
using WowsKarma.Api.Data;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Replays;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;
using WowsKarma.ServiceDefaults;

Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateBootstrapLogger();

Conversions.ConfigureMapping();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
if (!builder.Environment.IsEnvironment("Aspire"))
	builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);

Startup startup = new(builder);
builder.AddServiceDefaults();
builder.Host.UseSerilog((context, services, logger) =>
{
	logger.ReadFrom.Configuration(context.Configuration)
		.Enrich.WithProperty("_Region", Startup.ApiRegion.ToRegionString());

	if (context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] is { Length: > 0 })
		logger.WriteTo.OpenTelemetry();
}, preserveStaticLogger: true);

builder.Host.UseSystemd();

startup.ConfigureServices(builder.Services);

await using WebApplication host = builder.Build();
startup.Configure(host);

await using AsyncServiceScope scope = host.Services.CreateAsyncScope();

await using (ApiDbContext db = scope.ServiceProvider.GetRequiredService<ApiDbContext>())
{
	await db.Database.MigrateAsync();
}

await using (AuthDbContext db = scope.ServiceProvider.GetRequiredService<AuthDbContext>())
{
	await db.Database.MigrateAsync();
}

string storageSection = $"Api:{Startup.ApiRegion.ToRegionString()}:Azure:Storage";
if (builder.Configuration.GetValue<bool>($"{storageSection}:InitializeContainers"))
{
	BlobServiceClient storage = new(builder.Configuration[$"{storageSection}:ConnectionString"]);
	await storage.GetBlobContainerClient(ReplaysIngestService.ReplayBlobContainer).CreateIfNotExistsAsync(PublicAccessType.Blob);
	await storage.GetBlobContainerClient(ReplaysIngestService.SecurityBlobContainer).CreateIfNotExistsAsync();
	await storage.GetBlobContainerClient(MinimapRenderingService.MinimapBlobContainer).CreateIfNotExistsAsync(PublicAccessType.Blob);
}

Log.Information("Region selected : {Region}", Startup.ApiRegion);
await host.RunAsync();
