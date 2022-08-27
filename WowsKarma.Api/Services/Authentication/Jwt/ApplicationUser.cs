using Microsoft.AspNetCore.Identity;

namespace WowsKarma.Api.Services.Authentication.Jwt;

public class ApplicationUser : IdentityUser<uint>
{
	public ApplicationUser() : base() { }
}