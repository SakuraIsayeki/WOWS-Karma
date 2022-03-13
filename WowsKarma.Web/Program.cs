using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Threading.Tasks;
using WowsKarma.Common;
using WowsKarma.Web.Infrastructure;

namespace WowsKarma.Web
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			using IHost host = CreateHostBuilder(args).Build();
			using IServiceScope scope = host.Services.CreateScope();

			IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

			Log.Logger = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Debug()
#else
				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.Enrich.WithProperty("_Source", typeof(Program).Assembly.GetName())
				.Enrich.WithProperty("_Environment", configuration["environment"])
				.Enrich.WithProperty("_Region", Utilities.CurrentRegion.ToRegionString())
				.WriteTo.Console()
#if DEBUG
				.WriteTo.Seq(configuration["Seq:ListenUrl"], apiKey: configuration["Seq:ApiKey"])
#endif
				.CreateLogger();

			Log.Information("Region selected : {Region}", Utilities.CurrentRegion);

			Conversions.ConfigureMapping();
			
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
							  .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
						config.AddEnvironmentVariables();
						config.AddCommandLine(args);
					});

					webBuilder.UseStartup<Startup>();
					webBuilder.UseStaticWebAssets();
					webBuilder.UseSerilog();
				});
	}
}
