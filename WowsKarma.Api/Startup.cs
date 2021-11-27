using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wargaming.WebAPI;
using Wargaming.WebAPI.Models;
using Wargaming.WebAPI.Requests;
using WowsKarma.Api.Data;
using WowsKarma.Api.Hubs;
using WowsKarma.Api.Infrastructure.Telemetry;
using WowsKarma.Api.Middlewares;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Api.Services.Discord;
using WowsKarma.Api.Services.Replays;
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
				.AddNewtonsoftJsonProtocol(options =>
				{
					options.PayloadSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
				})
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

						options.Events = new JwtBearerEvents
						{
							OnMessageReceived = context =>
							{
								StringValues accessToken = context.Request.Query["access_token"];

								// If the request is for our hub...
								if (accessToken != StringValues.Empty && context.Request.Path.StartsWithSegments("/api/hubs"))
								{
									// Read the token out of the query string
									context.Token = accessToken;
								}

								return Task.CompletedTask;
							}
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



			services.AddApplicationInsightsTelemetry(options =>
			{
#if DEBUG
				options.DeveloperMode = true;
#endif
			});

			string dbConnectionString = $"ApiDbConnectionString:{ApiRegion.ToRegionString()}";
			int dbPoolSize = Configuration.GetValue<int>("Database:PoolSize");

			services.AddDbContextPool<ApiDbContext>(
				options => options.UseNpgsql(Configuration.GetConnectionString(dbConnectionString),
					providerOptions => providerOptions.EnableRetryOnFailure()), dbPoolSize is 0 ? 64 : dbPoolSize);

			services.AddDbContextPool<AuthDbContext>(
				options => options.UseNpgsql(Configuration.GetConnectionString(dbConnectionString),
					providerOptions => providerOptions.EnableRetryOnFailure()), dbPoolSize is 0 ? 64 : dbPoolSize);

			services.AddHttpClient<WorldOfWarshipsHandler>(client => client.BaseAddress = new(ApiProperties.GetApiHost(ApiProperties.Game.WOWS, ApiRegion)));
			services.AddHttpClient<VortexApiHandler>(client => client.BaseAddress = new(VortexApiHandler.GetApiHost(ApiRegion)));

			services.AddWargamingAuth();
			services.AddScoped<JwtAuthenticationHandler>();

			services.AddSingleton<JwtService>();
			services.AddSingleton<JwtSecurityTokenHandler>();
			services.AddSingleton(new WorldOfWarshipsHandlerOptions(ApiRegion, Configuration[$"Api:{ApiRegion.ToRegionString()}:AppId"]));
			services.AddSingleton<WorldOfWarshipsHandler>();
			services.AddSingleton<VortexApiHandler>();
			services.AddSingleton<PostWebhookService>();
			services.AddSingleton<ModActionWebhookService>();
			services.AddSingleton<ITelemetryInitializer, TelemetryEnrichment>();


			services.AddTransient<PostHub>();

			services.AddScoped<UserService>();
			services.AddScoped<PlayerService>();
			services.AddScoped<PostService>();
			services.AddScoped<KarmaService>();
			services.AddScoped<ModService>();
			services.AddScoped<NotificationService>();
			services.AddScoped<ReplaysIngestService>();

			services.AddApplicationInsightsTelemetryProcessor<HubTelemetryFilter>();

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


			IEnumerable<IPAddress> allowedProxies = Configuration.GetSection("AllowedProxies")?.Get<string[]>()?.Select(x => IPAddress.Parse(x));

			// Nginx configuration step
			ForwardedHeadersOptions forwardedHeadersOptions = new()
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			};

			if (allowedProxies is not null && allowedProxies.Any())
			{
				forwardedHeadersOptions.KnownProxies.Clear();

				foreach (IPAddress address in allowedProxies)
				{
					forwardedHeadersOptions.KnownProxies.Add(address);
				}
			}

			app.UseForwardedHeaders(forwardedHeadersOptions);

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseMiddleware<RequestLoggingMiddleware>();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
				endpoints.MapHub<PostHub>("/api/hubs/post");
				endpoints.MapHub<NotificationsHub>("/api/hubs/notifications");
			});
		}
	}
}
