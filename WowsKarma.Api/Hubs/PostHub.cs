using Mapster;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Services;
using WowsKarma.Common;
using WowsKarma.Common.Hubs;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Hubs
{
	public class PostHub : Hub<IPostHubPush>, IPostHubInvoke
	{

	}
}
