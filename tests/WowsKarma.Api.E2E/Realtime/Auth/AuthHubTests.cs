using System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Realtime.Auth;

/// <summary>Anonymous SignalR connections and seed revocation events.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("AuthHub")]
public sealed class AuthHubTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("Renewing an account seed broadcasts the invalidation on the auth hub.")]
    public async Task BroadcastsSeedRevocation()
    {
        var account = await fixture.AccountAsync();
        using var client = fixture.Client(account);
        await using var connection = new HubConnectionBuilder().WithUrl(new Uri(client.BaseAddress!, "/api/hubs/auth")).Build();
        var invalidated = new TaskCompletionSource<uint>(TaskCreationOptions.RunContinuationsAsynchronously);
        // The hub broadcasts revocations from other tests, too. Match only this account.
        connection.On<uint>("SeedTokenInvalidated", id => { if (id == account.Id) invalidated.TrySetResult(id); });
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await connection.StartAsync(timeout.Token);
        await Expect(await client.PostAsync("/api/auth/renew-seed", null), HttpStatusCode.OK);
        Check(await invalidated.Task.WaitAsync(timeout.Token) == account.Id, "Seed invalidation event changed");
    }

    [Test]
    [DisplayName("Anonymous clients can connect to the auth hub.")]
    public async Task ConnectsAnonymously()
    {
        using var client = fixture.Client();
        await using var connection = new HubConnectionBuilder().WithUrl(new Uri(client.BaseAddress!, "/api/hubs/auth")).Build();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await connection.StartAsync(timeout.Token);
        Check(connection.State == HubConnectionState.Connected, "SignalR connection failed");
    }
}
