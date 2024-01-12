using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nodsoft.WowsReplaysUnpack;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Tags.PostgreSql;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Nodsoft.Wargaming.Api.Client;
using Nodsoft.Wargaming.Api.Client.Clients;
using Nodsoft.Wargaming.Api.Client.Clients.Wows;
using Nodsoft.Wargaming.Api.Common;
using Nodsoft.WowsReplaysUnpack.ExtendedData;
using Npgsql;
using WowsKarma.Api.Data;
using WowsKarma.Api.Hubs;
using WowsKarma.Api.Infrastructure.Authorization;
using WowsKarma.Api.Infrastructure.Telemetry;
using WowsKarma.Api.Middlewares;
using WowsKarma.Api.Minimap.Client;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Api.Services.Authentication.Cookie;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Api.Services.Discord;
using WowsKarma.Api.Services.Posts;
using WowsKarma.Api.Services.Replays;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;

namespace WowsKarma.Api;

public sealed class Startup
{
	public static Region ApiRegion { get; private set; }
	public static string DisplayVersion { get; private set; } = "0.0.0";
	public IConfiguration Configuration { get; }


	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
		ApiRegion = Common.Utilities.GetRegionConfigString(Configuration["Api:CurrentRegion"] ?? "EU");
		DisplayVersion = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
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
				options.PayloadSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
			})
			.AddMessagePackProtocol();

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			// Add JWT Bearer Authentication
			.AddScheme<JwtBearerOptions, ForwardCookieAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme,
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
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]!)),
					};

					options.Events = new()
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
				}
			);



		services.AddAuthorization(options =>
		{
			options.AddPolicy(RequireNoPlatformBans, policy =>
			{
				policy.Requirements.Add(new PlatformBanRequirement());
			});
		});
			
			
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc(DisplayVersion, new()
			{
				Version = DisplayVersion,
				Title = $"WOWS Karma API ({ApiRegion.ToRegionString()})",
				Contact = new()
				{
					Name = "Sakura Isayeki",
					Email = "sakura.isayeki@nodsoft.net",
					Url = new("https://github.com/SakuraIsayeki"),
				},
				License = new()
				{
					Name = "GNU-GPL v3",
					Url = new("https://github.com/SakuraIsayeki/WoWS-Karma/blob/main/LICENSE"),
				}
			});

			// Set the comments path for the Swagger JSON and UI.
			string xmlFile = $"{typeof(Startup).Assembly.GetName().Name}.xml";
			string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
			options.IncludeXmlComments(xmlPath);

			// Bearer token authentication
			options.AddSecurityDefinition("jwt_auth", new()
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

			options.AddSecurityRequirement(new()
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

		NpgsqlDataSource apiDbDataSourceBuilder = new NpgsqlDataSourceBuilder(Configuration.GetConnectionString(dbConnectionString))
			.ConfigureApiDbDataSourceBuilder()
			.Build();
		
		services.AddDbContextPool<ApiDbContext>(
			o => o.UseNpgsql(apiDbDataSourceBuilder,
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
			        ?? throw new InvalidOperationException("AppId not found in configuration"),
		});
		
		services.AddWowsReplayUnpacker(builder =>
		{
			builder.AddExtendedData();
		});

		services.AddMinimapApiClient(Configuration.GetSection("MinimapApi")); 
		
		services.AddHangfireServer();
		services.AddHangfire((_, config) =>
		{
			config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
			config.UseSimpleAssemblyNameTypeSerializer();
				
			config.UseRecommendedSerializerSettings(options =>
			{
				options.TypeNameHandling = TypeNameHandling.Auto;
			});

			config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(Configuration.GetConnectionString(dbConnectionString)));
			//config.UsePostgreSqlStorage(Configuration.GetConnectionString(dbConnectionString), new() { SchemaName = "hangfire", PrepareSchemaIfNecessary = true });
			
			config.UseSerilogLogProvider();
			config.UseTagsWithPostgreSql();
		});
			
		services.AddWargamingAuth();
		services.AddScoped<JwtAuthenticationHandler>();

		services.AddSingleton<JwtService>();
		services.AddSingleton<JwtSecurityTokenHandler>();
		services.AddSingleton<PostWebhookService>();
		services.AddSingleton<ModActionWebhookService>();
		services.AddSingleton<ITelemetryInitializer, TelemetryEnrichment>();

		services.AddTransient<PostHub>();

		services.AddScoped<ClanService>();
		services.AddScoped<UserService>();
		services.AddScoped<PlayerService>();
		services.AddScoped<PostService>();
		services.AddScoped<PostUpdatesBroadcastService>();
		services.AddScoped<KarmaService>();
		services.AddScoped<ModService>();
		services.AddScoped<NotificationService>();
		services.AddScoped<ReplaysIngestService>();
		services.AddScoped<ReplaysProcessService>();
		services.AddScoped<MinimapRenderingService>();

		services.AddScoped<IAuthorizationHandler, PlatformBanAuthorizationHandler>();

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
		app.UseETagger();
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
				.AllowAnyMethod()
				.WithExposedPaginationHeaders();

			builder.AllowCredentials();
		});

		IPAddress[] allowedProxies = Configuration.GetSection("AllowedProxies").Get<string[]>()?.Select(IPAddress.Parse).ToArray() ?? [];

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
				
			endpoints.MapHangfireDashboard("/hangfire", new()
			{
				AppPath = ApiRegion.GetRegionWebDomain(),
				Authorization = [ HangfireDashboardAuthorizationFilter.Instance ],
				IsReadOnlyFunc = HangfireDashboardAuthorizationFilter.IsAccessReadOnly,
				DashboardTitle = $"WOWS Karma API ({ApiRegion.ToRegionString()})"
			});
				
			endpoints.MapHub<AuthHub>("/api/hubs/auth");
			endpoints.MapHub<PostHub>("/api/hubs/post");
			endpoints.MapHub<NotificationsHub>("/api/hubs/notifications");
		});
	}
}