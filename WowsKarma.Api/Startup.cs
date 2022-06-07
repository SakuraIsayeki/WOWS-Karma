using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nodsoft.WowsReplaysUnpack;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Nodsoft.Wargaming.Api.Client;
using Nodsoft.Wargaming.Api.Client.Clients;
using Nodsoft.Wargaming.Api.Client.Clients.Wows;
using Nodsoft.Wargaming.Api.Common;
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
			DisplayVersion = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		}


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			services.AddExceptionHandler(
				options =>
				{
					options.ExceptionHandlingPath = "/error";
					options.AllowStatusCode404Response = true;
				});
			
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
				o => o.UseNpgsql(Configuration.GetConnectionString(dbConnectionString),
					p =>
					{
						p.EnableRetryOnFailure();
					}
				), 
				dbPoolSize is 0 ? 64 : dbPoolSize);

			services.AddDbContextPool<AuthDbContext>(
				o => o.UseNpgsql(Configuration.GetConnectionString(dbConnectionString),
					p =>
					{
						p.EnableRetryOnFailure();
					}
				), 
				dbPoolSize is 0 ? 64 : dbPoolSize);

			services.AddThrottledApiClient<WowsPublicApiClient>((_, client) => client.BaseAddress = new(ApiHostUtilities.GetApiHost(Game.WOWS, ApiRegion)), 20);
			services.AddApiClient<WowsVortexApiClient>((_, client) => client.BaseAddress = new(WowsVortexApiClient.GetApiHost(ApiRegion)));
			services.AddApiClient<WowsClansApiClient>((_, client) => client.BaseAddress = new(WowsClansApiClient.GetApiHost(ApiRegion)));
			
			services.AddSingleton(s => new PublicApiOptions
			{
				AppId = s.GetRequiredService<IConfiguration>()[$"Api:{ApiRegion.ToRegionString()}:AppId"]
			});
			
			services.AddWargamingAuth();
			services.AddScoped<JwtAuthenticationHandler>();

			services.AddSingleton<JwtService>();
			services.AddSingleton<JwtSecurityTokenHandler>();
			services.AddSingleton<PostWebhookService>();
			services.AddSingleton<ModActionWebhookService>();
			services.AddSingleton<ITelemetryInitializer, TelemetryEnrichment>();
			services.AddSingleton<ReplayUnpacker>();
			
			services.AddTransient<PostHub>();

			services.AddScoped<ClanService>();
			services.AddScoped<UserService>();
			services.AddScoped<PlayerService>();
			services.AddScoped<PostService>();
			services.AddScoped<KarmaService>();
			services.AddScoped<ModService>();
			services.AddScoped<NotificationService>();
			services.AddScoped<ReplaysIngestService>();
			services.AddScoped<ReplaysProcessService>();

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

			app.UseExceptionHandler();

			app.UseRouting();
			
			// Allow CORS (permissive)
			app.UseCors(builder =>
			{
				builder.SetIsOriginAllowed(_ => true); // Allow all origins

				builder.AllowAnyHeader()
					.AllowAnyMethod();

				builder.AllowCredentials();
			});

			IPAddress[] allowedProxies = Configuration.GetSection("AllowedProxies")?.Get<string[]>()?.Select(IPAddress.Parse).ToArray();

			// Nginx configuration step
			ForwardedHeadersOptions forwardedHeadersOptions = new()
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			};

			if (allowedProxies is { Length: not 0 })
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
				endpoints.MapHub<AuthHub>("/api/hubs/auth");
				endpoints.MapHub<PostHub>("/api/hubs/post");
				endpoints.MapHub<NotificationsHub>("/api/hubs/notifications");
			});
		}
	}
}
