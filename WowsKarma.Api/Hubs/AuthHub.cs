using Microsoft.AspNetCore.SignalR;
using WowsKarma.Common.Hubs;

namespace WowsKarma.Api.Hubs;

public sealed class AuthHub : Hub<IAuthHubPush>, IAuthHubInvoke
{

}
