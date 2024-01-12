using Microsoft.AspNetCore.SignalR;
using WowsKarma.Common.Hubs;

namespace WowsKarma.Api.Hubs;

public sealed class PostHub : Hub<IPostHubPush>, IPostHubInvoke
{

}