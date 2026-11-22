using System.Net;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Http.Auth;

/// <summary>JWT validation, token lifecycle, and regional OpenID authentication.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Auth")]
public sealed class AuthTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("Bearer and cookie tokens authenticate, and renewing the seed revokes both old and refreshed tokens.")]
    public async Task RefreshesAndRevokesTokens()
    {
        var account = await fixture.AccountAsync();
        using var client = fixture.Client(account);
        using var cookie = fixture.Client(account, cookie: true);
        await Expect(await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/api/auth")), HttpStatusCode.OK);
        await Expect(await cookie.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/api/auth")), HttpStatusCode.OK);
        using var refreshed = await client.GetAsync("/api/auth/refresh-token");
        refreshed.EnsureSuccessStatusCode();
        var token = (await refreshed.Content.ReadAsStringAsync()).Trim('"');
        using var newClient = fixture.Client(account with { Token = token });
        await Expect(await newClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/api/auth")), HttpStatusCode.OK);

        // Refresh preserves the account seed, so renewing it must invalidate both tokens.
        await Expect(await client.PostAsync("/api/auth/renew-seed", null), HttpStatusCode.OK);
        await Expect(await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/api/auth")), HttpStatusCode.Unauthorized);
        await Expect(await newClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/api/auth")), HttpStatusCode.Unauthorized);
    }

    [Test]
    [DisplayName("An invalid JWT does not authenticate.")]
    public async Task RejectsInvalidToken()
    {
        using var client = fixture.Client(new(1, "invalid.jwt.token"));
        await Expect(await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/api/auth")), HttpStatusCode.Unauthorized);
    }

    [Test]
    [DisplayName("Login redirects to the EU identity provider with the configured callback.")]
    public async Task RedirectsToRegionalOpenId()
    {
        using var addressClient = fixture.Client();
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = addressClient.BaseAddress };
        using var response = await client.GetAsync("/api/auth/login?redirectUri=https%3A%2F%2Fexample.invalid%2Fafter-login");
        Check(response.StatusCode == HttpStatusCode.Redirect, "Login must redirect to OpenID");
        var location = response.Headers.Location!;
        Check(location.Host == "eu.wargaming.net" && location.AbsolutePath == "/id/openid", "Wrong regional OpenID provider");
        Check(Uri.UnescapeDataString(location.Query).Contains("http://localhost/api/auth/wg-callback"), "Missing callback URL");
    }

    [Test]
    [DisplayName("A verified callback redirects and issues a cookie accepted by the API.")]
    public async Task AuthenticatesOpenIdCallback()
    {
        using var address = fixture.Client();
        // Read the original redirect and pass the issued cookie back explicitly.
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { BaseAddress = address.BaseAddress };
        using var response = await client.GetAsync("/api/auth/wg-callback?openid.identity=https%3A%2F%2Feu.wargaming.net%2Fid%2F900001-FixtureSailor%2F&redirectUri=%2Fafter-login");
        Check(response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location!.OriginalString == "/after-login", "Callback redirect changed");
        var cookie = response.Headers.GetValues("Set-Cookie").Single().Split(';')[0];
        Check(cookie.StartsWith("Auth_EU=", StringComparison.Ordinal), "Callback auth cookie missing");
        client.DefaultRequestHeaders.Add("Cookie", cookie);
        await Expect(await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/api/auth")), HttpStatusCode.OK);
    }

    [Test]
    [DisplayName("An unverified OpenID callback is forbidden.")]
    public async Task RejectsInvalidOpenIdCallback()
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync("/api/auth/wg-callback"), HttpStatusCode.Forbidden);
    }

    [Test]
    [DisplayName("$method $path requires an authenticated account")]
    [Arguments("HEAD", "/api/auth")]
    [Arguments("POST", "/api/auth/renew-seed")]
    [Arguments("GET", "/api/auth/refresh-token")]
    public async Task RequiresAuthentication(string method, string path)
    {
        using var client = fixture.Client();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        await Expect(await client.SendAsync(request), HttpStatusCode.Unauthorized);
    }
}
