using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Api.E2E.Support.ApiAssert;
using static WowsKarma.Api.E2E.Support.PostRequests;

namespace WowsKarma.Api.E2E.Http.Posts;

/// <summary>Post lifecycle, visibility, write restrictions, and pagination.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Posts")]
public sealed class PostTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("Creating, editing, and deleting a post applies and reverses the recipient's karma.")]
    public async Task UpdatesKarmaThroughPostLifecycle()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        using var client = fixture.Client(author);
        var post = await CreateOk(client, Draft(author, target));
        var fetched = await client.GetFromJsonAsync<PlayerPostDTO>($"/api/post/{post.Id}");
        Check(fetched?.Title == post.Title && fetched?.Author?.Id == author.Id && fetched?.Player?.Id == target.Id, "Post DTO round trip changed");
        Check(await Karma(client, target.Id) == 1, "Positive post should increase karma");

        // Switching the same post from positive to negative requires reversing its previous effect.
        await Expect(await client.PutAsJsonAsync("/api/post", post with { Title = "Edited baseline", Flairs = PostFlairs.PerformanceBad }), HttpStatusCode.OK);
        fetched = await client.GetFromJsonAsync<PlayerPostDTO>($"/api/post/{post.Id}");
        Check(fetched!.Title == "Edited baseline" && fetched.Flairs == PostFlairs.PerformanceBad, "Edit not persisted");
        Check(await Karma(client, target.Id) == -1, "Edit should reverse positive karma and apply negative karma");
        await Expect(await client.DeleteAsync($"/api/post/{post.Id}"), HttpStatusCode.ResetContent);
        await Expect(await client.GetAsync($"/api/post/{post.Id}"), HttpStatusCode.NotFound);
        Check(await Karma(client, target.Id) == 0, "Deletion should reverse karma");
    }

    [Test]
    [DisplayName("An author edits a seeded post and reads back the updated title and content.")]
    public async Task EditsPost()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        var id = await fixture.PostAsync(author, target);
        using var client = fixture.Client(author);
        var edited = Draft(author, target) with { Id = id, Title = "Independent edit", Flairs = PostFlairs.Neutral };
        await Expect(await client.PutAsJsonAsync("/api/post", edited), HttpStatusCode.OK);
        var read = await client.GetFromJsonAsync<PlayerPostDTO>($"/api/post/{id}");
        Check(read!.Title == edited.Title && read.Content == edited.Content, "Edit not persisted");
    }

    [Test]
    [DisplayName("An author deletes a seeded post and subsequent reads return 404.")]
    public async Task DeletesPost()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        var id = await fixture.PostAsync(author, target);
        using var client = fixture.Client(author);
        await Expect(await client.DeleteAsync($"/api/post/{id}"), HttpStatusCode.ResetContent);
        await Expect(await client.GetAsync($"/api/post/{id}"), HttpStatusCode.NotFound);
    }

    [Test]
    [DisplayName("The author and a different account cannot change or delete a locked post.")]
    public async Task RejectsLockedPostWrites()
    {
        var author = await fixture.AccountAsync();
        var other = await fixture.AccountAsync();
        var id = await fixture.PostAsync(author, other, locked: true);
        var draft = Draft(author, other) with { Id = id };
        using var stranger = fixture.Client(other);
        using var owner = fixture.Client(author);
        // The stranger fails the ownership check. The author fails the lock check.
        foreach (var client in new[] { stranger, owner })
        {
            await Expect(await client.PutAsJsonAsync("/api/post", draft), HttpStatusCode.BadRequest);
            await Expect(await client.DeleteAsync($"/api/post/{id}"), HttpStatusCode.BadRequest);
        }
    }

    [Test]
    [DisplayName("Post writes enforce ownership, content limits, and privileges for bypassing validation.")]
    public async Task EnforcesWriteConstraints()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        var other = await fixture.AccountAsync();
        using var client = fixture.Client(author);
        using var stranger = fixture.Client(other);
        var draft = Draft(author, target);
        await Expect(await Create(stranger, draft), HttpStatusCode.BadRequest);
        await Expect(await Create(client, draft, "?ignoreChecks=true"), HttpStatusCode.BadRequest);
        await Expect(await Create(client, draft with { Title = "" }), HttpStatusCode.BadRequest);
        await Expect(await Create(client, draft with { Title = new string('x', 61) }), HttpStatusCode.BadRequest);
        await Expect(await Create(client, draft with { Content = new string('x', 2001) }), HttpStatusCode.BadRequest);
        var post = draft with { Id = await fixture.PostAsync(author, target) };
        await Expect(await stranger.PutAsJsonAsync("/api/post", post with { Title = "Unauthorized" }), HttpStatusCode.BadRequest);
        await Expect(await stranger.DeleteAsync($"/api/post/{post.Id}"), HttpStatusCode.BadRequest);
        await Expect(await client.PutAsJsonAsync("/api/post", post with { Id = Guid.Empty }), HttpStatusCode.NotFound);
        await Expect(await client.DeleteAsync($"/api/post/{Guid.Empty}"), HttpStatusCode.NotFound);
        var unchanged = await client.GetFromJsonAsync<PlayerPostDTO>($"/api/post/{post.Id}");
        Check(unchanged!.Title == draft.Title, "Rejected mutation changed post");
    }

    [Test]
    [DisplayName("A platform ban blocks the author and an opted-out recipient blocks submission.")]
    public async Task EnforcesPostingRestrictions()
    {
        var banned = await fixture.AccountAsync(banned: true);
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        using var bannedClient = fixture.Client(banned);
        await Expect(await Create(bannedClient, Draft(banned, target)), HttpStatusCode.Forbidden);
        using var client = fixture.Client(author);
        await using var db = fixture.Database();
        await db.Players.Where(p => p.Id == target.Id).ExecuteUpdateAsync(s => s.SetProperty(p => p.OptedOut, true));
        await Expect(await Create(client, Draft(author, target)), HttpStatusCode.BadRequest);
    }

    [Test]
    [DisplayName("Anonymous readers cannot see locked posts, and authors retain read access without write access.")]
    public async Task HidesLockedPosts()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        using var client = fixture.Client(author);
        var post = Draft(author, target) with { Id = await fixture.PostAsync(author, target) };
        await using var db = fixture.Database();
        await db.Posts.Where(p => p.Id == post.Id).ExecuteUpdateAsync(s => s.SetProperty(p => p.ModLocked, true));
        using var anonymous = fixture.Client();
        await Expect(await anonymous.GetAsync($"/api/post/{post.Id}"), HttpStatusCode.Gone);
        await Expect(await client.GetAsync($"/api/post/{post.Id}"), HttpStatusCode.OK);
        await Expect(await client.PutAsJsonAsync("/api/post", post), HttpStatusCode.BadRequest);
        await Expect(await client.DeleteAsync($"/api/post/{post.Id}"), HttpStatusCode.BadRequest);
        var received = await anonymous.GetFromJsonAsync<PlayerPostDTO[]>($"/api/post/{target.Id}/received");
        Check(received!.Length == 0, "Locked post leaked through received listing");
    }

    [Test]
    [DisplayName("Post listings include seeded IDs and return the requested page and pagination headers.")]
    public async Task PaginatesPostLists()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        using var client = fixture.Client(author);
        var post = Draft(author, target) with { Id = await fixture.PostAsync(author, target) };
        Check((await client.GetFromJsonAsync<Guid[]>("/api/post"))!.Contains(post.Id!.Value), "Post ID absent");
        foreach (var route in new[] { $"{author.Id}/sent", $"{target.Id}/received" })
        {
            using var response = await client.GetAsync($"/api/post/{route}?page=1&pageSize=1");
            response.EnsureSuccessStatusCode();
            Check(response.Headers.GetValues("Content-Items-Total").Single() == "1", "Pagination total changed");
            Check(response.Headers.GetValues("Content-Page-Current").Single() == "1", "Pagination page changed");
            Check((await response.Content.ReadFromJsonAsync<PlayerPostDTO[]>())!.Single().Id == post.Id, "Wrong paginated post");
        }
        using var latest = await client.GetAsync("/api/post/latest?pageSize=1&hasReplay=false");
        latest.EnsureSuccessStatusCode();
        Check((await latest.Content.ReadFromJsonAsync<PlayerPostDTO[]>())!.Length == 1, "Latest page size changed");
    }

    [Test]
    [DisplayName("GET $path returns $status for invalid or missing resources")]
    [Arguments("/api/post/00000000-0000-0000-0000-000000000000", 404)]
    [Arguments("/api/post/not-a-guid", 404)]
    public async Task HandlesInvalidResource(string path, int status)
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync(path), (HttpStatusCode)status);
    }

    [Test]
    [DisplayName("Anonymous requests cannot create, edit, or delete posts.")]
    public async Task RejectsAnonymousWrites()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        using var client = fixture.Client();
        await Expect(await Create(client, Draft(author, target)), HttpStatusCode.Unauthorized);
        await Expect(await client.PutAsJsonAsync("/api/post", Draft(author, target)), HttpStatusCode.Unauthorized);
        await Expect(await client.DeleteAsync($"/api/post/{Guid.Empty}"), HttpStatusCode.Unauthorized);
    }

    [Test]
    [DisplayName("Malformed JSON in a multipart post is rejected.")]
    public async Task RejectsMalformedPayload()
    {
        var author = await fixture.AccountAsync();
        using var client = fixture.Client(author);
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("not json"), "postDto");
        await Expect(await client.PostAsync("/api/post", form), HttpStatusCode.BadRequest);
    }

    private static async Task<int> Karma(HttpClient client, uint id)
    {
        using var result = await client.PostAsJsonAsync("/api/player/karmas", new[] { id });
        result.EnsureSuccessStatusCode();
        return (await result.Content.ReadFromJsonAsync<Dictionary<uint, int>>())![id];
    }
}
