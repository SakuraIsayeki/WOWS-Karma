using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using WowsKarma.Common;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Http.Mod;

/// <summary>Moderator authorization and post action lifecycle.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Mod")]
public sealed class ModActionTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("Public action queries distinguish missing results from a missing filter.")]
    public async Task ValidatesActionQueries()
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync($"/api/mod/action/{Guid.Empty}"), HttpStatusCode.NoContent);
        await Expect(await client.GetAsync("/api/mod/action/list"), HttpStatusCode.BadRequest);
        await Expect(await client.GetAsync("/api/mod/action/list?userId=4294967295"), HttpStatusCode.NoContent);
        await Expect(await client.GetAsync($"/api/mod/action/list?postId={Guid.NewGuid()}"), HttpStatusCode.NoContent);
    }

    [Test]
    [DisplayName("A community manager cannot moderate a post they wrote.")]
    public async Task RejectsSelfModeration()
    {
        var moderator = await fixture.AccountAsync(false, ApiRoles.CM);
        var target = await fixture.AccountAsync();
        var id = await fixture.PostAsync(moderator, target);
        using var client = fixture.Client(moderator);
        await Expect(await client.PostAsJsonAsync("/api/mod/action", new PostModActionDTO { PostId = id, Reason = "Self relation", ActionType = ModActionType.Deletion }), HttpStatusCode.Forbidden);
        await using var db = fixture.Database();
        Check(!await db.PostModActions.AnyAsync(a => a.PostId == id), "Rejected moderation persisted an action");
    }

    [Test]
    [DisplayName("A moderator locks a post, reads the action, and reverts it to unlock the post.")]
    public async Task LocksAndUnlocksPost()
    {
        var moderator = await fixture.AccountAsync(false, ApiRoles.CM);
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        var postId = await fixture.PostAsync(author, target);
        using var client = fixture.Client(moderator);
        await Expect(await client.PostAsJsonAsync("/api/mod/action", new PostModActionDTO { PostId = postId, Reason = "Baseline moderation", ActionType = ModActionType.Deletion }), HttpStatusCode.Accepted);
        var actions = (await client.GetFromJsonAsync<PostModActionDTO[]>($"/api/mod/action/list?postId={postId}"))!;
        Check(actions is [{ Reason: "Baseline moderation" }], "Moderation listing changed");
        var action = await client.GetFromJsonAsync<PostModActionDTO>($"/api/mod/action/{actions[0].Id}");
        Check(action!.PostId == postId, "Moderation action readback changed");
        await Expect(await client.DeleteAsync($"/api/mod/action/{action.Id}"), HttpStatusCode.ResetContent);

        // Verify both persisted changes without relying on the separate post DTO mapping path.
        await using var db = fixture.Database();
        Check(!await db.PostModActions.AnyAsync(a => a.Id == action.Id), "Reverted action still present");
        Check(!(await db.Posts.SingleAsync(p => p.Id == postId)).ModLocked, "Reversion did not unlock the post");
    }

    [Test]
    [DisplayName("$method $path requires a moderator role")]
    [Arguments("POST", "/api/mod/action")]
    [Arguments("DELETE", "/api/mod/action/00000000-0000-0000-0000-000000000001")]
    public async Task RequiresModerator(string method, string path)
    {
        var user = await fixture.AccountAsync();
        using var anonymous = fixture.Client();
        using var member = fixture.Client(user);
        await Expect(await anonymous.SendAsync(new HttpRequestMessage(new HttpMethod(method), path)), HttpStatusCode.Unauthorized);
        await Expect(await member.SendAsync(new HttpRequestMessage(new HttpMethod(method), path)), HttpStatusCode.Forbidden);
    }
}
