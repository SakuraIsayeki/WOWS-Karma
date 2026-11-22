using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Http.Clans;

/// <summary>Cached clan details and search through the Wargaming client.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Clans")]
public sealed class ClanTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("A seeded clan appears in the list and returns an empty member collection when requested.")]
    public async Task ReadsCachedClan()
    {
        var account = await fixture.AccountAsync();
        await using var db = fixture.Database();
        // Fresh timestamps keep the request on the cached clan path.
        var clan = new WowsKarma.Api.Data.Models.Clan { Id = account.Id, Name = "Baseline clan", Tag = "E2E", UpdatedAt = DateTimeOffset.UtcNow, MembersUpdatedAt = DateTimeOffset.UtcNow };
        db.Clans.Add(clan);
        await db.SaveChangesAsync();
        using var client = fixture.Client();
        Check((await client.GetFromJsonAsync<uint[]>("/api/clan"))!.Contains(clan.Id), "Clan missing from listing");
        using var result = await client.GetAsync($"/api/clan/{clan.Id}?includeMembers=false");
        result.EnsureSuccessStatusCode();
        var json = await result.Content.ReadFromJsonAsync<JsonElement>();
        Check(json.GetProperty("tag").GetString() == "E2E", "Clan tag changed");
        using var members = await client.GetAsync($"/api/clan/{clan.Id}?includeMembers=true");
        members.EnsureSuccessStatusCode();
        Check((await members.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("members").GetArrayLength() == 0, "Empty clan members changed");
    }

    [Test]
    [DisplayName("Clan search maps the upstream fixture and honors a result limit of zero.")]
    public async Task SearchesClans()
    {
        using var client = fixture.Client();
        var clans = await client.GetFromJsonAsync<ClanListingDTO[]>("/api/clan/search/Fixture");
        Check(clans is [{ Id: 900002, Tag: "FXT", Name: "Fixture clan" }], "Clan search contract changed");
        Check((await client.GetFromJsonAsync<ClanListingDTO[]>("/api/clan/search/Fixture?results=0"))!.Length == 0, "Zero search limit changed");
    }

    [Test]
    [DisplayName("GET $path returns $status for invalid or missing resources")]
    [Arguments("/api/clan/search/a", 400)]
    [Arguments("/api/clan/search/test?results=501", 400)]
    public async Task HandlesInvalidResource(string path, int status)
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync(path), (HttpStatusCode)status);
    }

    [Test]
    [DisplayName("The clan endpoint handles an upstream 404.")]
    public async Task HandlesMissingUpstream()
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync("/api/clan/4294967295"), HttpStatusCode.NoContent);
    }
}
