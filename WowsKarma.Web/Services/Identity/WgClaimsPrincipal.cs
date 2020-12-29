using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WowsKarma.Web.Services.Identity
{
	public class WgClaimsPrincipal : ClaimsPrincipal
	{
		public WgClaimsPrincipal(ClaimsPrincipal principal) : base(principal)
		{
			
		}
	}
}
