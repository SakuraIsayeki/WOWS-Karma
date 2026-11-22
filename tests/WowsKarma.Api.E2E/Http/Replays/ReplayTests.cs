using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TUnit.Core;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.E2E.Support;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs.Replays;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Http.Replays;

/// <summary>Replay metadata, upload restrictions, and administrative replay jobs.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Replays")]
public sealed class ReplayTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("Replay metadata links the replay to its post and exposes download and minimap URLs.")]
    public async Task ReadsReplayMetadata()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        using var client = fixture.Client(author);
        // Seed parsed metadata so this read test can run independently of replay ingestion.
        var postId = await fixture.PostAsync(author, target);
        var replayId = Guid.NewGuid();
        await using var db = fixture.Database();
        db.Replays.Add(new Replay { Id = replayId, PostId = postId, BlobName = "baseline.wowsreplay", ArenaInfo = JsonDocument.Parse("{}"), MinimapRendered = true });
        await db.SaveChangesAsync();
        await db.Posts.Where(p => p.Id == postId).ExecuteUpdateAsync(s => s.SetProperty(p => p.ReplayId, replayId));
        var ids = await client.GetFromJsonAsync<Guid[]>("/api/replay");
        Check(ids!.Contains(replayId), "Replay listing missing ID");
        var replay = await client.GetFromJsonAsync<ReplayDTO>($"/api/replay/{replayId}");
        Check(replay!.Id == replayId && replay.PostId == postId, "Replay identity changed");
        Check(replay.DownloadUri!.EndsWith("/replays/baseline.wowsreplay"), "Replay download link changed");
        Check(replay.MinimapUri!.EndsWith($"/{replayId}.mp4"), "Minimap link changed");
    }

    [Test]
    [DisplayName("Replay upload rejects missing posts and posts written by another account.")]
    public async Task EnforcesUploadOwnership()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        using var client = fixture.Client(author);
        using var other = fixture.Client(target);
        var postId = await fixture.PostAsync(author, target);
        foreach (var (caller, id) in new[] { (client, Guid.Empty), (other, postId) })
        {
            using var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent([1, 2, 3]), "replay", "invalid.wowsreplay");
            await Expect(await caller.PostAsync($"/api/replay/{id}", form), HttpStatusCode.BadRequest);
        }
    }

    [Test]
    [DisplayName("An administrator can enqueue reprocessing for a replay ID and receives a location.")]
    public async Task EnqueuesReplayReprocessing()
    {
        var admin = await fixture.AccountAsync(false, WowsKarma.Common.ApiRoles.Administrator);
        using var client = fixture.Client(admin);
        using var response = await client.PatchAsync($"/api/replay/reprocess/replay/{Guid.NewGuid()}", null);
        Check(response.StatusCode == HttpStatusCode.Accepted && response.Headers.Location is not null, "Single replay job acceptance changed");
    }

    [Test]
    [DisplayName("For a post without a replay, the synchronous request completes and the async request is accepted.")]
    public async Task HandlesMinimapWithoutReplay()
    {
        var author = await fixture.AccountAsync();
        var target = await fixture.AccountAsync();
        var admin = await fixture.AccountAsync(false, WowsKarma.Common.ApiRoles.Administrator);
        var postId = await fixture.PostAsync(author, target);
        using var client = fixture.Client(admin);
        await Expect(await client.PatchAsync($"/api/replay/reprocess/minimap/{postId}?waitForCompletion=true", null), HttpStatusCode.OK);
        await Expect(await client.PatchAsync($"/api/replay/reprocess/minimap/{postId}", null), HttpStatusCode.Accepted);
    }

    [Test]
    [DisplayName("GET $path returns $status for invalid or missing resources")]
    [Arguments("/api/replay/00000000-0000-0000-0000-000000000000", 204)]
    public async Task HandlesInvalidResource(string path, int status)
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync(path), (HttpStatusCode)status);
    }

    [Test]
    [DisplayName("$method $path requires an authenticated account")]
    [Arguments("PATCH", "/api/replay/reprocess/replay/all")]
    [Arguments("PATCH", "/api/replay/reprocess/minimap/all")]
    [Arguments("PATCH", "/api/replay/reprocess/replay/00000000-0000-0000-0000-000000000001")]
    [Arguments("PATCH", "/api/replay/reprocess/minimap/00000000-0000-0000-0000-000000000001")]
    [Arguments("POST", "/api/replay/00000000-0000-0000-0000-000000000001")]
    public async Task RequiresAuthentication(string method, string path)
    {
        using var client = fixture.Client();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        await Expect(await client.SendAsync(request), HttpStatusCode.Unauthorized);
    }

    [Test]
    [DisplayName("Only an administrator can enqueue replay and minimap batch jobs.")]
    public async Task EnqueuesBatchJobs()
    {
        var user = await fixture.AccountAsync();
        var admin = await fixture.AccountAsync(false, ApiRoles.Administrator);
        using var client = fixture.Client(user);
        using var administrator = fixture.Client(admin);
        // A past date range keeps these acceptance checks independent of other tests' replays.
        foreach (var path in new[] { "/api/replay/reprocess/replay/all?start=2000-01-01&end=2000-01-02", "/api/replay/reprocess/minimap/all?start=2000-01-01&end=2000-01-02" })
        {
            await Expect(await client.PatchAsync(path, null), HttpStatusCode.Forbidden);
            await Expect(await administrator.PatchAsync(path, null), HttpStatusCode.Accepted);
        }
        await Expect(await administrator.PatchAsync($"/api/replay/reprocess/minimap/{Guid.Empty}", null), HttpStatusCode.NotFound);
    }
}
