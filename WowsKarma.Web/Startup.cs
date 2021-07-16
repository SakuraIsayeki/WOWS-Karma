using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using WowsKarma.Common;
using WowsKarma.Web.Middlewares;
using WowsKarma.Web.Services;
using WowsKarma.Web.Services.Authentication;
using static WowsKarma.Common.Utilities;
using static WowsKarma.Web.Utilities;

namespace WowsKarma.Web
{
	public class Startup
	{		
		public IConfiguration Configuration { get; }
		public static string DisplayVersion { get; private set; }

		public const string WgAuthScheme = "Wargaming";
		public const string CookieAuthScheme = "Cookie";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			DisplayVersion = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		}



		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			CurrentRegion = GetRegionConfigString(Configuration["Api:CurrentRegion"] ?? "EU");

			services.AddServerSideBlazor();
			services.AddRazorPages();
			services.AddHttpContextAccessor();

			services.AddDistributedMemoryCache();

			services.AddHttpClient(Options.DefaultName, config =>
			{
				config.BaseAddress = new(Configuration[$"Api:{CurrentRegion.ToRegionString()}:Host"]);
			});

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

			//TODO : Add custom Auth Handler
			services.AddAuthentication(ApiTokenAuthenticationHandler.AuthenticationScheme)
				.AddScheme<AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>(ApiTokenAuthenticationHandler.AuthenticationScheme, "API Token", options => { });

			services.AddAuthorizationCore();
			services.AddHttpContextAccessor();

			services.AddResponseCompression(opts =>
			{
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					new[] { "application/octet-stream" });
			});

			services.AddSingleton<JwtSecurityTokenHandler>();
			services.AddSingleton<PageContentLoader>();
			services.AddSingleton<PlayerService>();

			services.AddScoped<UserService>();
			services.AddScoped<PostService>();
			services.AddScoped<ModService>();
		}



		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseResponseCompression();

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

			app.UseMiddleware<RequestLoggingMiddleware>();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
