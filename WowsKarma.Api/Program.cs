using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Common;

namespace WowsKarma.Api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			using IHost host = CreateHostBuilder(args).Build();
			using IServiceScope scope = host.Services.CreateScope();

			ApiDbContext db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApiDbContext>>().CreateDbContext();
			IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

			await db.Database.MigrateAsync();

			Log.Logger = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
				.MinimumLevel.Information()
#endif
//				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.Enrich.WithProperty("_Source", typeof(Program).Assembly.GetName())
				.Enrich.WithProperty("_Environment", configuration["environment"])
				.Enrich.WithProperty("_Region", Startup.ApiRegion.ToRegionString())
				.WriteTo.Console()
				.WriteTo.Seq(configuration["Seq:ListenUrl"], apiKey: configuration["Seq:ApiKey"])
//				.WriteTo.Logger(fileLogger)
				.CreateLogger();

			Console.WriteLine("Region selected : {0}", Startup.ApiRegion);

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
					webBuilder.UseSerilog();
				});
	}
}
