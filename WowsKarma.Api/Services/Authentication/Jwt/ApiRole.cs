using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;

namespace WowsKarma.Api.Services.Authentication.Jwt;

[UsedImplicitly]
public sealed class ApiRole : IdentityRole
{
	public ApiRole() : base() { }
	public ApiRole(string name) : base(name)
	{
		Id = name;
	}
}