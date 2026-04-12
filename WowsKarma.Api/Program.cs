using Microsoft.EntityFrameworkCore;
using Serilog;
using WowsKarma.Api;
using WowsKarma.Api.Data;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;

Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateBootstrapLogger();

Conversions.ConfigureMapping();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Startup startup = new(builder);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
	.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);

builder.Services.AddSerilog(logger => logger
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.WithProperty("_Region", Startup.ApiRegion.ToRegionString())
);

builder.Host.UseSystemd();

startup.ConfigureServices(builder.Services);

await using WebApplication host = builder.Build();
startup.Configure(host);

await using AsyncServiceScope scope = host.Services.CreateAsyncScope();

await host.StartAsync();

Log.Information("Region selected : {Region}", Startup.ApiRegion);
await using (ApiDbContext db = scope.ServiceProvider.GetRequiredService<ApiDbContext>())
{
	await db.Database.MigrateAsync();
}

await using (AuthDbContext db = scope.ServiceProvider.GetRequiredService<AuthDbContext>())
{
	await db.Database.MigrateAsync();
}

await host.WaitForShutdownAsync();