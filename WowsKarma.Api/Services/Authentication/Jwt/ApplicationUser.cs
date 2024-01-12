using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;

namespace WowsKarma.Api.Services.Authentication.Jwt;

[UsedImplicitly]
public class ApplicationUser : IdentityUser<uint>
{
	public ApplicationUser() : base() { }
}