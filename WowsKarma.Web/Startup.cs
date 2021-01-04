using AspNet.Security.OpenId;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using WowsKarma.Web.Services;
using static WowsKarma.Common.Utilities;
using static WowsKarma.Web.Utilities;

namespace WowsKarma.Web
{
	public class Startup
	{
		public const string WgAuthScheme = "Wargaming";
		public const string CookieAuthScheme = "Cookie";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddServerSideBlazor();
			services.AddRazorPages();

			services.AddHttpClient(Options.DefaultName, config =>
			{
				config.BaseAddress = new(Configuration["Api:Host"]);
				config.DefaultRequestHeaders.Add("Access-Key", Configuration["Api:AccessKey"]);
			});

			services.AddSingleton<PlayerService>();
			services.AddSingleton<PostService>();

			services.AddApplicationInsightsTelemetry(options =>
			{
#if DEBUG
				options.DeveloperMode = true; 
#endif
			});
#if RELEASE
			services.AddHsts(options =>
			{
				options.Preload = true;
			});
#endif

			services.AddAuthentication(options =>
			{
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = WgAuthScheme;
			})
			.AddCookie()
			.AddOpenId(WgAuthScheme, "Wargaming.net", options =>
			{
				options.Authority = new(GetOidcEndpoint(GetRegionConfigString(Configuration["Api:Region"])));
				options.CallbackPath = OpenIdAuthenticationDefaults.CallbackPath;
			});

			services.AddAuthorizationCore();
			services.AddHttpContextAccessor();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

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

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
