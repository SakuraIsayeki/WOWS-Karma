using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
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
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public static Region ApiRegion { get; internal set; }
		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			ApiRegion = Common.Utilities.GetRegionConfigString(Configuration["Api:CurrentRegion"] ?? "EU");

			services.AddControllers();


			int dbPoolSize = Configuration.GetValue<int>("Database:PoolSize");

			services.AddPooledDbContextFactory<ApiDbContext>(
				options => options.UseNpgsql(Configuration.GetConnectionString($"ApiDbConnectionString:{ApiRegion.ToRegionString()}"),
				providerOptions => providerOptions.EnableRetryOnFailure()), dbPoolSize is 0 ? 16 : dbPoolSize);

			services.AddHttpClient<WorldOfWarshipsHandler>(client => client.BaseAddress = new(ApiProperties.GetApiHost(ApiProperties.Game.WOWS, ApiRegion)));
			services.AddHttpClient<VortexApiHandler>(client => client.BaseAddress = new(VortexApiHandler.GetApiHost(ApiRegion)));

			services.AddSingleton(new WorldOfWarshipsHandlerOptions(ApiRegion, Configuration[$"Api:{ApiRegion.ToRegionString()}:AppId"]));
			services.AddSingleton<WorldOfWarshipsHandler>();
			services.AddSingleton<VortexApiHandler>();

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
