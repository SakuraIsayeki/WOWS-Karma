using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Api.Services.Authentication.Jwt
{
	public class ApplicationUser : IdentityUser<uint>
	{
		public ApplicationUser() : base() { }
	}
}
