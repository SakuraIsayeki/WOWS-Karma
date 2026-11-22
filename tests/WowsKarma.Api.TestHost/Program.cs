using System.Collections;
using System.Reflection;
using Aspire.Hosting;

// No user secrets, fixed host ports, persistent volumes, or production services.
var builder = DistributedApplication.CreateBuilder(args);
var database = builder.AddPostgres("postgres").WithImageTag("17.6")
    .AddDatabase("api-db", "wowskarma_e2e");
var blobs = builder.AddAzureStorage("storage").RunAsEmulator().AddBlobs("replay-storage");

// Use the prebuilt API so Aspire does not rebuild an assembly prepared by the coverage collector.
var configuration = typeof(TestHostSettings).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
var apiAssembly = Path.GetFullPath(Path.Combine("../../WowsKarma.Api/bin", configuration, "net10.0", "WowsKarma.Api.dll"), builder.AppHostDirectory);
if (!File.Exists(apiAssembly))
{
    throw new FileNotFoundException("Build the API end-to-end test project before starting its AppHost.", apiAssembly);
}

builder.AddExecutable("api", "dotnet", builder.AppHostDirectory, apiAssembly)
    .WithEnvironment("ASPNETCORE_CONTENTROOT", builder.AppHostDirectory)
    .WithEnvironment("DOTNET_STARTUP_HOOKS", typeof(WowsKarma.Api.TestOverrides.ExternalServiceOverrides).Assembly.Location)
    .WithEnvironment("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", "WowsKarma.Api.TestOverrides")
    .WithEnvironment(context =>
    {
        // Aspire builds a separate environment for the API, so forward the active coverage session.
        if (Environment.GetEnvironmentVariable("CODE_COVERAGE_SESSION_NAME") is null)
        {
            return;
        }

        foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
        {
            var name = (string)variable.Key;
            if (name.StartsWith("CODE_COVERAGE_", StringComparison.Ordinal)
                || name.StartsWith("MicrosoftInstrumentationEngine_", StringComparison.Ordinal)
                || name.StartsWith("CORECLR_PROFILER", StringComparison.Ordinal)
                || name == "CORECLR_ENABLE_PROFILING")
            {
                context.EnvironmentVariables[name] = (string)variable.Value!;
            }
        }
    })
    .WithHttpEndpoint(env: "ASPNETCORE_HTTP_PORTS")
    .WithHttpHealthCheck("/api/status")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Aspire")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Aspire")
    .WithEnvironment("Api__CurrentRegion", "EU")
    .WithEnvironment("Api__EU__AppId", "e2e-unused")
    .WithEnvironment("Api__EU__CookieName", "Auth_EU")
    .WithEnvironment("Api__EU__CookieDomain", "localhost")
    .WithEnvironment("Api__EU__WgAuthCallback", "http://localhost/api/auth/wg-callback")
    .WithEnvironment("JWT__Secret", TestHostSettings.SigningKey)
    .WithEnvironment("JWT__ValidIssuer", "e2e")
    .WithEnvironment("JWT__ValidAudience", "e2e")
    .WithEnvironment("Aspire__DisableApplicationInsights", "true")
    .WithEnvironment("Serilog__WriteTo__0__Name", "Console")
    .WithEnvironment("Serilog__MinimumLevel__Default", "Warning")
    .WithEnvironment("Discord__WebhookAvatarPath", "https://example.invalid/avatar.png")
    .WithEnvironment("MinimapApi__BaseUrl", "http://127.0.0.1:1")
    .WithEnvironment("MinimapApi__Login", "unused")
    .WithEnvironment("MinimapApi__Password", "unused")
    .WithEnvironment("Api__EU__Azure__Storage__ConnectionString", blobs.Resource.ConnectionStringExpression)
    .WithEnvironment("Api__EU__Azure__Storage__InitializeContainers", "true")
    .WithReference(database, connectionName: "ApiDbConnectionString__EU")
    .WaitFor(database).WaitFor(blobs);

await builder.Build().RunAsync();

// TUnit uses this type to locate the test AppHost assembly.
public sealed class TestHostSettings
{
    private TestHostSettings() { }

    // Only accepted by the disposable test API process.
    public const string SigningKey = "wows-karma-e2e-signing-key-for-local-tests-only-2026";
}
