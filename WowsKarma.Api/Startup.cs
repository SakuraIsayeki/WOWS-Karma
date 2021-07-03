using Discord.Webhook;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Wargaming.WebAPI;
using Wargaming.WebAPI.Models;
using Wargaming.WebAPI.Requests;
using WowsKarma.Api.Data;
using WowsKarma.Api.Hubs;
using WowsKarma.Api.Middlewares;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Authentication.Jwt;
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
			services.AddSignalR()
				.AddMessagePackProtocol();

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

				// Adding Jwt Bearer  
				.AddScheme<JwtBearerOptions, JwtAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme, 
					options =>
					{
						options.SaveToken = true;
						options.RequireHttpsMetadata = false;
						options.TokenValidationParameters = new()
						{
							ValidateIssuer = true,
							ValidateAudience = true,
							ValidAudience = Configuration["JWT:ValidAudience"],
							ValidIssuer = Configuration["JWT:ValidIssuer"],
							IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
						};
					});

			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc(DisplayVersion, new OpenApiInfo
				{
					Version = DisplayVersion,
					Title = $"WOWS Karma API ({ApiRegion.ToRegionString()})",
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

				// Set the comments path for the Swagger JSON and UI.
				string xmlFile = $"{typeof(Startup).Assembly.GetName().Name}.xml";
				string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				options.IncludeXmlComments(xmlPath);

				// Bearer token authentication
				options.AddSecurityDefinition("jwt_auth", new OpenApiSecurityScheme()
				{
					Name = "bearer",
					BearerFormat = "JWT",
					Scheme = "bearer",
					Description = "Specify the authorization token.",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
				});

				// Make sure swagger UI requires a Bearer token specified
				OpenApiSecurityScheme securityScheme = new()
				{
					Reference = new()
					{
						Id = "jwt_auth",
						Type = ReferenceType.SecurityScheme
					}
				};

				options.AddSecurityRequirement(new OpenApiSecurityRequirement()
				{
					{ securityScheme, Array.Empty<string>() },
				});
			});

			string dbConnectionString = $"ApiDbConnectionString:{ApiRegion.ToRegionString()}";
			int dbPoolSize = Configuration.GetValue<int>("Database:PoolSize");

			services.AddPooledDbContextFactory<ApiDbContext>(
				options => options.UseNpgsql(Configuration.GetConnectionString(dbConnectionString),
					providerOptions => providerOptions.EnableRetryOnFailure()), dbPoolSize is 0 ? 64 : dbPoolSize);

			services.AddPooledDbContextFactory<AuthDbContext>(
				options => options.UseNpgsql(Configuration.GetConnectionString(dbConnectionString),
					providerOptions => providerOptions.EnableRetryOnFailure()), dbPoolSize is 0 ? 64 : dbPoolSize);

			services.AddHttpClient<WorldOfWarshipsHandler>(client => client.BaseAddress = new(ApiProperties.GetApiHost(ApiProperties.Game.WOWS, ApiRegion)));
			services.AddHttpClient<VortexApiHandler>(client => client.BaseAddress = new(VortexApiHandler.GetApiHost(ApiRegion)));

			services.AddWargamingAuth();

			services.AddSingleton<JwtService>();
			services.AddSingleton<JwtAuthenticationHandler>();
			services.AddSingleton<JwtSecurityTokenHandler>();
			services.AddSingleton(new WorldOfWarshipsHandlerOptions(ApiRegion, Configuration[$"Api:{ApiRegion.ToRegionString()}:AppId"]));
			services.AddSingleton<WorldOfWarshipsHandler>();
			services.AddSingleton<VortexApiHandler>();
			services.AddSingleton(new DiscordWebhookClient(Configuration[$"Webhooks:Discord:{ApiRegion.ToRegionString()}"]));

			services.AddTransient<PostHub>();

			services.AddScoped<UserService>();
			services.AddScoped<PlayerService>();
			services.AddScoped<PostService>();
			services.AddScoped<KarmaService>();

			services.AddApplicationInsightsTelemetry(options =>
			{
#if DEBUG
				options.DeveloperMode = true;
#endif
			});

			services.AddResponseCompression(opts =>
			{
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					new[] { "application/octet-stream" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseResponseCompression();

			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint($"/swagger/{DisplayVersion}/swagger.json", $"WOWS Karma API v{DisplayVersion}");
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
				endpoints.MapHub<PostHub>("/api/hubs/post");
			});
		}
	}
}
