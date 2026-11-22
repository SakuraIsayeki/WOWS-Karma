using System.Net;
using System.Net.Http.Json;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Http.Mod;

/// <summary>Platform ban listing, enforcement, and reversion.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Mod")]
public sealed class PlatformBanTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("A temporary ban blocks posting and revoking it clears the profile ban flag.")]
    public async Task AppliesAndRevertsBan()
    {
        var moderator = await fixture.AccountAsync(false, ApiRoles.CM);
        var target = await fixture.AccountAsync();
        var recipient = await fixture.AccountAsync();
        using var client = fixture.Client(moderator);
        await Expect(await client.PostAsJsonAsync("/api/mod/bans?days=2", new PlatformBanDTO { UserId = target.Id, Reason = "Baseline ban" }), HttpStatusCode.Accepted);
        var bans = (await client.GetFromJsonAsync<PlatformBanDTO[]>($"/api/mod/bans/{target.Id}?currentOnly=true"))!;
        Check(bans is [{ Reason: "Baseline ban" }] && bans[0].ModId == moderator.Id, "Ban data changed");
        Check(bans[0].BannedUntil > DateTimeOffset.UtcNow.AddDays(1), "Temporary ban expiry missing");
        using var banned = fixture.Client(target);
        await Expect(await PostRequests.Create(banned, PostRequests.Draft(target, recipient)), HttpStatusCode.Forbidden);
        await Expect(await client.DeleteAsync($"/api/mod/bans/{bans[0].Id}"), HttpStatusCode.ResetContent);
        var flags = await banned.GetFromJsonAsync<UserProfileFlagsDTO>($"/api/profile/{target.Id}");
        Check(!flags!.PostsBanned, "Reverted ban still restricts the account");
    }

    [Test]
    [DisplayName("Anonymous users can list bans with the default query options.")]
    public async Task ListsBansAnonymously()
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync("/api/mod/bans/4294967295"), HttpStatusCode.OK);
    }

    [Test]
    [DisplayName("An active-only listing excludes a reverted indefinite ban.")]
    public async Task ExcludesRevertedBans()
    {
        var user = await fixture.AccountAsync();
        var moderator = await fixture.AccountAsync(false, ApiRoles.CM);
        await using var db = fixture.Database();
        db.PlatformBans.Add(new WowsKarma.Api.Data.Models.PlatformBan { UserId = user.Id, ModId = moderator.Id, Reason = "Reverted ban", Reverted = true });
        await db.SaveChangesAsync();
        using var client = fixture.Client();
        var active = await client.GetFromJsonAsync<PlatformBanDTO[]>($"/api/mod/bans/{user.Id}?currentOnly=true");
        Check(active!.Length == 0, "Reverted ban included among active bans");
    }

    [Test]
    [DisplayName("$method $path requires a moderator role")]
    [Arguments("POST", "/api/mod/bans")]
    [Arguments("DELETE", "/api/mod/bans/00000000-0000-0000-0000-000000000001")]
    public async Task RequiresModerator(string method, string path)
    {
        var user = await fixture.AccountAsync();
        using var anonymous = fixture.Client();
        using var member = fixture.Client(user);
        await Expect(await anonymous.SendAsync(new HttpRequestMessage(new HttpMethod(method), path)), HttpStatusCode.Unauthorized);
        await Expect(await member.SendAsync(new HttpRequestMessage(new HttpMethod(method), path)), HttpStatusCode.Forbidden);
    }
}
