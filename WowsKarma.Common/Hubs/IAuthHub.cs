using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowsKarma.Common.Hubs;

public interface IAuthHubPush
{
	public Task SeedTokenInvalidated(uint accountId);
}

public interface IAuthHubInvoke
{

}
