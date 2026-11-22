using Microsoft.AspNetCore.SignalR.Client;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Realtime.Posts;

/// <summary>Anonymous SignalR connections to the post hub.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("PostHub")]
public sealed class PostHubTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("Anonymous clients can connect to the post hub.")]
    public async Task ConnectsAnonymously()
    {
        using var client = fixture.Client();
        await using var connection = new HubConnectionBuilder().WithUrl(new Uri(client.BaseAddress!, "/api/hubs/post")).Build();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await connection.StartAsync(timeout.Token);
        Check(connection.State == HubConnectionState.Connected, "SignalR connection failed");
    }
}
