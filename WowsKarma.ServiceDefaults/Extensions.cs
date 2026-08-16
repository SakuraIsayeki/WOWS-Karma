using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
	private const string HealthEndpointPath = "/health";
	private const string AlivenessEndpointPath = "/alive";

	extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
	{
		/// <summary>
		/// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
		/// </summary>
		/// <param name="builder">The host builder.</param>
		/// <typeparam name="TBuilder">The type of the host builder.</typeparam>
		/// <returns>The host builder.</returns>
		/// <param name="configureMeter">Optional action to configure the MeterProviderBuilder for metrics.</param>
		/// <param name="configureTracer">Optional action to configure the TracerProviderBuilder for tracing.</param>
		public TBuilder AddServiceDefaults(
			Action<MeterProviderBuilder>? configureMeter = null,
			Action<TracerProviderBuilder>? configureTracer = null
		) {
			builder.ConfigureOpenTelemetry();
			builder.ConfigureOtelSerilog();
			builder.AddDefaultHealthChecks();

			builder.Services.AddServiceDiscovery();

			builder.Services.ConfigureHttpClientDefaults(http =>
			{
				// Turn on resilience by default
				http.AddStandardResilienceHandler();

				// Turn on service discovery by default
				http.AddServiceDiscovery();
			});

			// Uncomment the following to restrict the allowed schemes for service discovery.
			builder.Services.Configure<ServiceDiscoveryOptions>(options =>
			{
			    options.AllowedSchemes = ["https"];
			});

			return builder;
		}
		
		/// <summary>
		/// Configures OpenTelemetry and Serilog for the application.
		/// </summary>
		/// <param name="builder">The host builder.</param>
		/// <typeparam name="TBuilder">The type of the host builder.</typeparam>
		/// <returns>The host builder.</returns>
		public TBuilder ConfigureOtelSerilog()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.CreateBootstrapLogger();
		
			builder.Services.AddSerilog((services, lc) => lc
				.ReadFrom.Configuration(builder.Configuration)
				.ReadFrom.Services(services)
				.Enrich.FromLogContext()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.WithClientIp()
				.Enrich.WithDemystifiedStackTraces()
				// .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
				// Temporary fix for Aspire newlines
				.WriteTo.Console(theme: ConsoleTheme.None)
				.WriteTo.OpenTelemetry(options =>
				{
					options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
					string[] headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]?.Split(',') ?? [];
				
					foreach (string header in headers)
					{
						(string key, string value) = header.Split('=') switch
						{
							[{ } k, { } v] => (k, v),
							var v => throw new($"Invalid header format {v}")
						};

						options.Headers.Add(key, value);
					}

					options.ResourceAttributes.Add("service.name", "api");
				}));
		
			return builder;
		}
		
		/// <summary>
		/// Configures OpenTelemetry for the application.
		/// </summary>
		/// <param name="builder">The host builder.</param>
		/// <typeparam name="TBuilder">The type of the host builder.</typeparam>
		/// <param name="configureMeter">Optional action to configure the MeterProviderBuilder for metrics.</param>
		/// <param name="configureTracer">Optional action to configure the TracerProviderBuilder for tracing.</param>
		/// <returns>The host builder.</returns>
		public TBuilder ConfigureOpenTelemetry(
			Action<MeterProviderBuilder>? configureMeter = null,
			Action<TracerProviderBuilder>? configureTracer = null
		) {
			builder.Logging.AddOpenTelemetry(logging =>
			{
				logging.IncludeFormattedMessage = true;
				logging.IncludeScopes = true;
			});

			builder.Services.AddOpenTelemetry()
				.WithMetrics(metrics =>
				{
					metrics.AddAspNetCoreInstrumentation()
						.AddHttpClientInstrumentation()
						.AddRuntimeInstrumentation();
				
					configureMeter?.Invoke(metrics);
				})
				.WithTracing(tracing =>
				{
					tracing.AddSource(builder.Environment.ApplicationName)
						.AddAspNetCoreInstrumentation()
						// Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
						//.AddGrpcClientInstrumentation()
						.AddHttpClientInstrumentation();
				
					configureTracer?.Invoke(tracing);
				});

			builder.AddOpenTelemetryExporters();

			return builder;
		}

		/// <summary>
		/// Adds OpenTelemetry exporters to the application.
		/// </summary>
		/// <param name="builder">The host builder.</param>
		/// <typeparam name="TBuilder">The type of the host builder.</typeparam>
		/// <returns>The host builder.</returns>
		private TBuilder AddOpenTelemetryExporters()
		{
			bool useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

			if (useOtlpExporter)
			{
				builder.Services.AddOpenTelemetry().UseOtlpExporter();
			}

			// Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
			//if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
			//{
			//    builder.Services.AddOpenTelemetry()
			//       .UseAzureMonitor();
			//}

			return builder;
		}

		/// <summary>
		/// Adds default health checks to the application.
		/// </summary>
		/// <param name="builder">The host builder.</param>
		/// <typeparam name="TBuilder">The type of the host builder.</typeparam>
		/// <returns>The host builder.</returns>
		public TBuilder AddDefaultHealthChecks()
		{
			builder.Services.AddRequestTimeouts(
				configure: static timeouts =>
					timeouts.AddPolicy("HealthChecks", TimeSpan.FromSeconds(5)));

			builder.Services.AddOutputCache(
				configureOptions: static caching =>
					caching.AddPolicy("HealthChecks",
						build: static policy => policy.Expire(TimeSpan.FromSeconds(10))));

			builder.Services.AddHealthChecks()
				// Add a default liveness check to ensure app is responsive
				.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

			return builder;
		}
	}

	/// <param name="app">The IApplicationBuilder to configure.</param>
	extension(WebApplication app)
	{
		/// <summary>
		/// Maps default health check endpoints to the application.
		/// </summary>
		/// <param name="app">The application builder.</param>
		/// <returns>The application builder.</returns>
		public WebApplication MapDefaultEndpoints()
		{
			// Adding health checks endpoints to applications in non-development environments has security implications.
			// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
			if (app.Environment.IsDevelopment())
			{
				// All health checks must pass for app to be considered ready to accept traffic after starting
				app.MapHealthChecks(HealthEndpointPath);

				// Only health checks tagged with the "live" tag must pass for app to be considered alive
				app.MapHealthChecks(AlivenessEndpointPath, new()
				{
					Predicate = r => r.Tags.Contains("live")
				});
			}

			return app;
		}
	
		/// <summary>
		/// Configures the WebApplication to log requests via Serilog.
		/// </summary>
		/// <param name="app">The application builder.</param>
		/// <returns>The application builder.</returns>
		public WebApplication UseSerilogRequestLogging()
		{
			// Request Logging
			app.UseSerilogRequestLogging(options =>
			{
				// Customize the message template
				options.MessageTemplate =
					"{RequestScheme} {RequestMethod} {RequestPath} by {RequestClient} responded {StatusCode} in {Elapsed:0.0000} ms";

				// Attach additional properties to the request completion event
				options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
				{
					diagnosticContext.Set("RequestClient", httpContext.Connection.RemoteIpAddress);
					diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme.ToUpperInvariant());
				};
			});
        
			return app;
		}

		/// <summary>
		/// Configures the application to support reverse proxy headers,
		/// allowing it to correctly identify client IP addresses and protocols when behind a reverse proxy.
		/// </summary>
		/// <param name="allowedProxies">An optional collection of IP addresses representing trusted reverse proxies. If provided, only headers from these proxies will be processed.</param>
		/// <returns>The configured IApplicationBuilder.</returns>
		public WebApplication ConfigureReverseProxySupport(IReadOnlyCollection<IPAddress>? allowedProxies = null)
		{
			ForwardedHeadersOptions forwardedHeadersOptions = new()
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			};

			if (allowedProxies is { Count: > 0 })
			{
				forwardedHeadersOptions.KnownProxies.Clear();

				foreach (IPAddress address in allowedProxies)
				{
					forwardedHeadersOptions.KnownProxies.Add(address);
				}
			}
        
			app.UseForwardedHeaders(forwardedHeadersOptions);
			return app;
		}
	}
}