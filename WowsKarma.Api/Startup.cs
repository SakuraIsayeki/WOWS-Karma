using Discord.Webhook;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using Wargaming.WebAPI;
using Wargaming.WebAPI.Models;
using Wargaming.WebAPI.Requests;
using WowsKarma.Api.Data;
using WowsKarma.Api.Middlewares;
using WowsKarma.Api.Services;
using WowsKarma.Common;

namespace WowsKarma.Api
{
	public class Startup
	{
		public static Region ApiRegion { get; private set; }
		public static string DisplayVersion { get; private set; }
		public IConfiguration Configuration { get; }


		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			ApiRegion = Common.Utilities.GetRegionConfigString(Configuration["Api:CurrentRegion"] ?? "EU");
			DisplayVersion = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		}


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			services.AddSwaggerGen();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc(DisplayVersion, new OpenApiInfo
				{
					Version = DisplayVersion,
					Title = "WOWS Karma API",
					Contact = new OpenApiContact
					{
						Name = "Sakura Isayeki",
						Email = "sakura.isayeki@nodsoft.net",
						Url = new Uri("https://github.com/SakuraIsayeki"),
					},
					License = new OpenApiLicense
					{
						Name = "GNU-GPL v3",
						Url = new Uri("https://github.com/SakuraIsayeki/WoWS-Karma/blob/main/LICENSE"),
					}
				});
			});


			int dbPoolSize = Configuration.GetValue<int>("Database:PoolSize");

			services.AddPooledDbContextFactory<ApiDbContext>(
				options => options.UseNpgsql(Configuration.GetConnectionString($"ApiDbConnectionString:{ApiRegion.ToRegionString()}"),
				providerOptions => providerOptions.EnableRetryOnFailure()), dbPoolSize is 0 ? 64 : dbPoolSize);

			services.AddHttpClient<WorldOfWarshipsHandler>(client => client.BaseAddress = new(ApiProperties.GetApiHost(ApiProperties.Game.WOWS, ApiRegion)));
			services.AddHttpClient<VortexApiHandler>(client => client.BaseAddress = new(VortexApiHandler.GetApiHost(ApiRegion)));

			services.AddSingleton(new WorldOfWarshipsHandlerOptions(ApiRegion, Configuration[$"Api:{ApiRegion.ToRegionString()}:AppId"]));
			services.AddSingleton<WorldOfWarshipsHandler>();
			services.AddSingleton<VortexApiHandler>();
			services.AddSingleton(new DiscordWebhookClient(Configuration[$"Webhooks:Discord:{ApiRegion.ToRegionString()}"]));

			services.AddScoped<PlayerService>();
			services.AddScoped<PostService>();
			services.AddScoped<KarmaService>();

			services.AddApplicationInsightsTelemetry(options =>
			{
#if DEBUG
				options.DeveloperMode = true;
#endif
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint($"/swagger/{DisplayVersion}/swagger.json", $"WOWS Karma v{DisplayVersion}");
			});

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			if (env.IsProduction()) // Nginx configuration step
			{
				app.UseForwardedHeaders(new ForwardedHeadersOptions
				{
					ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
				});
			}

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseMiddleware<RequestLoggingMiddleware>();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
			});
		}
	}
}
