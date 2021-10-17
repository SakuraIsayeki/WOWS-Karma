using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Common.Hubs
{
	public interface IPostHubPush
	{
		Task NewPost(PlayerPostDTO post);
		Task EditedPost(PlayerPostDTO edited);
		Task DeletedPost(Guid postId);
	}

	public interface IPostHubInvoke { }
}
