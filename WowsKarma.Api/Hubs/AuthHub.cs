using Microsoft.AspNetCore.SignalR;
using WowsKarma.Common.Hubs;

namespace WowsKarma.Api.Hubs;

public class AuthHub : Hub<IAuthHubPush>, IAuthHubInvoke
{

}
