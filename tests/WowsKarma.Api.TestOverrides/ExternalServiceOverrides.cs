using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

[assembly: HostingStartup(typeof(WowsKarma.Api.TestOverrides.ExternalServiceOverrides))]

// Loading this test assembly as a startup hook makes it available to hosting startup
// discovery without adding a reference or a test switch to the production API.
internal static class StartupHook
{
    public static void Initialize() { }
}

namespace WowsKarma.Api.TestOverrides
{
    public sealed class ExternalServiceOverrides : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder) => builder.ConfigureServices(services =>
            services.ConfigureAll<HttpClientFactoryOptions>(options =>
                options.HttpMessageHandlerBuilderActions.Add(http => http.AdditionalHandlers.Insert(0, new WargamingFixtureHandler()))));
    }

    internal sealed class WargamingFixtureHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!;
            if (!(uri.Host.EndsWith(".wargaming.net", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Contains(".worldofwarships.", StringComparison.OrdinalIgnoreCase)))
                return base.SendAsync(request, cancellationToken);

            var query = QueryHelpers.ParseQuery(uri.Query);
            var path = uri.AbsolutePath;
            if (path.EndsWith("/id/openid", StringComparison.Ordinal))
            {
                var valid = query.TryGetValue("openid.identity", out var identity)
                    && identity.ToString() == "https://eu.wargaming.net/id/900001-FixtureSailor/";
                return Reply(request, $"is_valid:{valid.ToString().ToLowerInvariant()}", "text/plain");
            }
            if (path.EndsWith("/account/list/", StringComparison.Ordinal))
                return Reply(request, query["search"] == "Fixture"
                    ? """{"status":"ok","meta":{"count":1},"data":[{"account_id":900001,"nickname":"FixtureSailor"}]}"""
                    : """{"status":"ok","meta":{"count":0},"data":[]}""");
            if (path.EndsWith("/search/clans/", StringComparison.Ordinal))
                return Reply(request, query["search"] == "Fixture" && query["limit"] != "0"
                    ? """{"clans":[{"id":900002,"tag":"FXT","name":"Fixture clan","hex_color":"#123456"}]}"""
                    : """{"clans":[]}""");

            // Explicitly absent resources exercise missing-player/clan responses. Any
            // additional upstream behavior must be given an intentional fixture here.
            if (path.Contains("/accounts/", StringComparison.Ordinal) || path.Contains("/clanbase/", StringComparison.Ordinal))
                return Reply(request, "{}", status: HttpStatusCode.NotFound);
            throw new InvalidOperationException($"No Wargaming fixture for {request.Method} {uri.AbsolutePath}");
        }

        private static Task<HttpResponseMessage> Reply(HttpRequestMessage request, string content, string mediaType = "application/json", HttpStatusCode status = HttpStatusCode.OK)
            => Task.FromResult(new HttpResponseMessage(status)
            {
                RequestMessage = request, Content = new StringContent(content, Encoding.UTF8, mediaType)
            });
    }
}
