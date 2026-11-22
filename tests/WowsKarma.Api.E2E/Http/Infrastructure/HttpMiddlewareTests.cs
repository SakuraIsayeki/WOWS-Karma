using System.Net;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Http.Infrastructure;

/// <summary>Conditional requests and credentialed CORS behavior.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Infrastructure")]
public sealed class HttpMiddlewareTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("Sending the ETag from a player response returns 304 with no body.")]
    public async Task SupportsConditionalGet()
    {
        var account = await fixture.AccountAsync();
        using var client = fixture.Client();
        var path = $"/api/player/{account.Id}?includeClanInfo=false";
        using var first = await client.GetAsync(path);
        first.EnsureSuccessStatusCode();
        Check(first.Headers.ETag is not null, "Cacheable response has no ETag");

        // Repeat the request with the server's validator to check the conditional response.
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.IfNoneMatch.Add(first.Headers.ETag!);
        using var conditional = await client.SendAsync(request);
        Check(conditional.StatusCode == HttpStatusCode.NotModified && (await conditional.Content.ReadAsByteArrayAsync()).Length == 0, "Conditional GET contract changed");
    }

    [Test]
    [DisplayName("CORS preflight allows a credentialed frontend to send a profile update.")]
    public async Task AllowsCredentialedPreflight()
    {
        using var client = fixture.Client();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/profile");
        request.Headers.Add("Origin", "https://frontend.example.invalid");
        request.Headers.Add("Access-Control-Request-Method", "PUT");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");
        using var response = await client.SendAsync(request);
        Check(response.StatusCode == HttpStatusCode.NoContent, "Preflight status changed");
        Check(response.Headers.GetValues("Access-Control-Allow-Origin").Single() == "https://frontend.example.invalid", "Credentialed origin missing");
        Check(response.Headers.GetValues("Access-Control-Allow-Credentials").Single() == "true", "Credentialed CORS changed");
    }
}
