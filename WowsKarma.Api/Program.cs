using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using WowsKarma.Api.Data;

namespace WowsKarma.Api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			RootCommand rootCommand = new("WOWS Karma - API")
			{
				new Option<string>("--region", () => "EU", "WG Region to cover [EU,NA,RU,ASIA]")
			};
			rootCommand.Handler = CommandHandler.Create<string>((region) =>
			{
				Startup.ApiRegion = Common.Utilities.GetRegionConfigString(region);
			});
			await rootCommand.InvokeAsync(args);


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
					webBuilder.UseStartup<Startup>();
					webBuilder.UseSerilog();
				});
	}
}
