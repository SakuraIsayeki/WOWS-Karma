using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using Serilog.Events;
using WowsKarma.Api.Data;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;

namespace WowsKarma.Api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			Conversions.ConfigureMapping();
			
			using IHost host = CreateHostBuilder(args).Build();
			using IServiceScope scope = host.Services.CreateScope();
			
			IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

			Log.Logger = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Debug()
#else
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
#endif
				.Enrich.FromLogContext()
				.Enrich.WithProperty("_Source", typeof(Program).Assembly.GetName())
				.Enrich.WithProperty("_Environment", configuration["environment"])
				.Enrich.WithProperty("_Region", Startup.ApiRegion.ToRegionString())
				.WriteTo.Console()
#if DEBUG
				.WriteTo.Seq(configuration["Seq:ListenUrl"], apiKey: configuration["Seq:ApiKey"])
#endif
				.CreateLogger();

			Log.Information("Region selected : {Region}", Startup.ApiRegion);
			
			
			using (ApiDbContext db = scope.ServiceProvider.GetRequiredService<ApiDbContext>())
			{
				await db.Database.MigrateAsync();
			}

			using (AuthDbContext db = scope.ServiceProvider.GetRequiredService<AuthDbContext>())
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
					});

					webBuilder.UseStartup<Startup>();
					webBuilder.UseSerilog();
				});
	}
}
