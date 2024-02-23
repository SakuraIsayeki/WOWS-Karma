using Microsoft.EntityFrameworkCore;
using Serilog;
using WowsKarma.Api.Data;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;

namespace WowsKarma.Api;

public sealed class Program
{
	public static async Task Main(string[] args)
	{
		Conversions.ConfigureMapping();

		using IHost host = CreateHostBuilder(args).Build();
		using IServiceScope scope = host.Services.CreateScope();
		
		Log.Information("Region selected : {region}", Startup.ApiRegion);
		await using (ApiDbContext db = scope.ServiceProvider.GetRequiredService<ApiDbContext>())
		{
			await db.Database.MigrateAsync();
		}

		await using (AuthDbContext db = scope.ServiceProvider.GetRequiredService<AuthDbContext>())
		{
			await db.Database.MigrateAsync();
		}


		await host.RunAsync();
	}

	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
					{
						config.SetBasePath(Directory.GetCurrentDirectory());
						config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
							.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
							.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
						config.AddEnvironmentVariables();
						config.AddCommandLine(args);
					}
				);

				webBuilder.UseStartup<Startup>();
			})
			.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
				.ReadFrom.Configuration(hostingContext.Configuration)
				.Enrich.WithProperty("_Region", Startup.ApiRegion.ToRegionString())
			)
			.UseSystemd();
}