using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Http.Profiles;

/// <summary>Profile flag persistence, validation, and ownership.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Profiles")]
public sealed class ProfileTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("Self-service opt-out persists, ban flags remain protected, and the cooldown rejects a second change.")]
    public async Task EnforcesProfileFlagRules()
    {
        var account = await fixture.AccountAsync();
        var other = await fixture.AccountAsync();
        using var client = fixture.Client(account);
        await Expect(await client.PutAsJsonAsync("/api/profile", new UserProfileFlagsDTO { Id = other.Id, OptedOut = true }), HttpStatusCode.BadRequest);
        await Expect(await client.PutAsJsonAsync("/api/profile", new UserProfileFlagsDTO { Id = account.Id, OptedOut = true, PostsBanned = true }), HttpStatusCode.NoContent);
        var flags = await client.GetFromJsonAsync<UserProfileFlagsDTO>($"/api/profile/{account.Id}");
        Check(flags!.OptedOut && !flags.PostsBanned, "Opt-out should persist; clients must not set their own ban flags");
        // Pin the last change to now to exercise the cooldown without waiting.
        await using var db = fixture.Database();
        await db.Players.Where(p => p.Id == account.Id).ExecuteUpdateAsync(s => s.SetProperty(p => p.OptedOut, true).SetProperty(p => p.OptOutChanged, DateTimeOffset.UtcNow));
        await Expect(await client.PutAsJsonAsync("/api/profile", new UserProfileFlagsDTO { Id = account.Id, OptedOut = false }), (HttpStatusCode)423);
    }

    [Test]
    [DisplayName("GET $path returns $status for invalid or missing resources")]
    [Arguments("/api/profile/0", 404)]
    public async Task HandlesInvalidResource(string path, int status)
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync(path), (HttpStatusCode)status);
    }

    [Test]
    [DisplayName("Anonymous requests cannot update profile flags.")]
    public async Task RejectsAnonymousWrites()
    {
        var author = await fixture.AccountAsync();
        using var client = fixture.Client();
        await Expect(await client.PutAsJsonAsync("/api/profile", new UserProfileFlagsDTO { Id = author.Id }), HttpStatusCode.Unauthorized);
    }

    [Test]
    [DisplayName("Malformed profile JSON is rejected.")]
    public async Task RejectsMalformedPayload()
    {
        var author = await fixture.AccountAsync();
        using var client = fixture.Client(author);
        using var json = new StringContent("{invalid", System.Text.Encoding.UTF8, "application/json");
        await Expect(await client.PutAsync("/api/profile", json), HttpStatusCode.BadRequest);
    }

    [Test]
    [DisplayName("The profile endpoint handles an upstream 404.")]
    public async Task HandlesMissingUpstream()
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync("/api/profile/4294967295"), HttpStatusCode.NotFound);
    }
}
