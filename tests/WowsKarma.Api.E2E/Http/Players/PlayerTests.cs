using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Http.Players;

/// <summary>Player discovery, profiles, karma, and metric recalculation.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Players")]
public sealed class PlayerTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("A seeded player appears in listings, profiles, and both karma responses.")]
    public async Task ReadsPlayersAndKarma()
    {
        var account = await fixture.AccountAsync();
        using var client = fixture.Client();
        var ids = await client.GetFromJsonAsync<uint[]>("/api/player");
        Check(ids!.Contains(account.Id), "Seeded player missing from listing");
        var profile = await client.GetFromJsonAsync<PlayerProfileDTO>($"/api/player/{account.Id}?includeClanInfo=false");
        Check(profile!.Id == account.Id && profile.Username == $"E2E_{account.Id}", "Player DTO changed");
        using var karmas = await client.PostAsJsonAsync("/api/player/karmas", new[] { account.Id, uint.MaxValue });
        karmas.EnsureSuccessStatusCode();
        var map = (await karmas.Content.ReadFromJsonAsync<Dictionary<uint, int>>())!;
        Check(map.Count == 1 && map[account.Id] == 0, "Karma lookup should omit unknown IDs");
        using var full = await client.PostAsJsonAsync("/api/player/karmas-full", new[] { account.Id });
        full.EnsureSuccessStatusCode();
        Check((await full.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength() == 1, "Full karma result missing");
        using var empty = await client.PostAsJsonAsync("/api/player/karmas", Array.Empty<uint>());
        empty.EnsureSuccessStatusCode();
        Check((await empty.Content.ReadFromJsonAsync<Dictionary<uint, int>>())!.Count == 0, "Empty karma query changed");
    }

    [Test]
    [DisplayName("Player search maps the upstream fixture and returns 204 for an empty result.")]
    public async Task SearchesPlayers()
    {
        using var client = fixture.Client();
        var players = await client.GetFromJsonAsync<AccountListingDTO[]>("/api/player/search/Fixture");
        Check(players is [{ Id: 900001, Username: "FixtureSailor" }], "Player search contract changed");
        await Expect(await client.GetAsync("/api/player/search/NoResults"), HttpStatusCode.NoContent);
    }

    [Test]
    [DisplayName("GET $path returns $status for invalid or missing resources")]
    [Arguments("/api/player/0", 400)]
    [Arguments("/api/player/not-a-number", 400)]
    [Arguments("/api/player/search/ab", 400)]
    [Arguments("/api/player/search/invalid-name", 400)]
    public async Task HandlesInvalidResource(string path, int status)
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync(path), (HttpStatusCode)status);
    }

    [Test]
    [DisplayName("$method $path requires an authenticated account")]
    [Arguments("PATCH", "/api/player/recalculate?playerId=1")]
    public async Task RequiresAuthentication(string method, string path)
    {
        using var client = fixture.Client();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        await Expect(await client.SendAsync(request), HttpStatusCode.Unauthorized);
    }

    [Test]
    [DisplayName("The player endpoint handles an upstream 404.")]
    public async Task HandlesMissingUpstream()
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync("/api/player/4294967295?includeClanInfo=false"), HttpStatusCode.NotFound);
    }

    [Test]
    [DisplayName("A regular user cannot enqueue a recalculation, while an administrator can.")]
    public async Task EnqueuesRecalculation()
    {
        var user = await fixture.AccountAsync();
        var admin = await fixture.AccountAsync(false, ApiRoles.Administrator);
        using var client = fixture.Client(user);
        using var administrator = fixture.Client(admin);
        var path = $"/api/player/recalculate?playerId={user.Id}";
        await Expect(await client.PatchAsync(path, null), HttpStatusCode.Forbidden);
        await Expect(await administrator.PatchAsync(path, null), HttpStatusCode.Accepted);
    }
}
