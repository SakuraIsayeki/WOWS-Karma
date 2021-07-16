using Microsoft.AspNetCore.Identity;

namespace WowsKarma.Api.Services.Authentication.Jwt
{
	public class ApiRole : IdentityRole
	{
		public ApiRole() : base() { }
		public ApiRole(string name) : base(name)
		{
			Id = name;
		}
	}
}
