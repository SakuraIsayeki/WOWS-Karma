using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Realtime.Notifications;

/// <summary>Notification hub authentication, streaming, and acknowledgement.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Notifications")]
public sealed class NotificationHubTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("A new account can read an empty stream and acknowledge an empty set.")]
    public async Task HandlesEmptyNotifications()
    {
        var account = await fixture.AccountAsync();
        using var client = fixture.Client();
        await using var connection = new HubConnectionBuilder().WithUrl(new Uri(client.BaseAddress!, "/api/hubs/notifications"),
            options => options.AccessTokenProvider = () => Task.FromResult<string?>(account.Token)).Build();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await connection.StartAsync(timeout.Token);
        var count = 0;
        await foreach (var _ in connection.StreamAsync<JsonElement>("GetPendingNotifications", timeout.Token)) count++;
        Check(count == 0, "New account has unexpected notifications");
        await connection.InvokeAsync("AcknowledgeNotifications", Array.Empty<Guid>(), timeout.Token);
    }

    [Test]
    [DisplayName("Only the caller's notifications appear in the stream or can be acknowledged.")]
    public async Task ScopesNotificationsToAccount()
    {
        var account = await fixture.AccountAsync();
        var other = await fixture.AccountAsync();
        var ownId = Guid.NewGuid();
        var foreignId = Guid.NewGuid();
        await using (var db = fixture.Database())
        {
            var ban = new WowsKarma.Api.Data.Models.PlatformBan { UserId = account.Id, ModId = other.Id, Reason = "Baseline notification", Reverted = true };
            db.PlatformBans.Add(ban);
            await db.SaveChangesAsync();
            db.PlatformBanNotifications.AddRange(
                new() { Id = ownId, AccountId = account.Id, BanId = ban.Id },
                new() { Id = foreignId, AccountId = other.Id, BanId = ban.Id });
            await db.SaveChangesAsync();
        }
        using var client = fixture.Client();
        await using var connection = new HubConnectionBuilder().WithUrl(new Uri(client.BaseAddress!, "/api/hubs/notifications"),
            options => options.AccessTokenProvider = () => Task.FromResult<string?>(account.Token)).Build();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await connection.StartAsync(timeout.Token);
        var pending = new List<JsonElement>();
        await foreach (var item in connection.StreamAsync<JsonElement>("GetPendingNotifications", timeout.Token)) pending.Add(item);
        Check(pending.Count == 1 && pending[0].GetRawText().Contains(ownId.ToString()), "Pending notifications leaked or omitted account data");
        // Include another account's ID to verify the server applies its own ownership filter.
        await connection.InvokeAsync("AcknowledgeNotifications", new[] { ownId, foreignId }, timeout.Token);
        await using var verification = fixture.Database();
        Check((await verification.Notifications.SingleAsync(n => n.Id == ownId)).AcknowledgedAt is not null, "Own notification not acknowledged");
        Check((await verification.Notifications.SingleAsync(n => n.Id == foreignId)).AcknowledgedAt is null, "Another account's notification was acknowledged");
    }

    [Test]
    [DisplayName("$method $path requires an authenticated account")]
    [Arguments("POST", "/api/hubs/notifications/negotiate?negotiateVersion=1")]
    public async Task RequiresAuthentication(string method, string path)
    {
        using var client = fixture.Client();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        await Expect(await client.SendAsync(request), HttpStatusCode.Unauthorized);
    }
}
