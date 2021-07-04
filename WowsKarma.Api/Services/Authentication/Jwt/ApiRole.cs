using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Api.Services.Authentication.Jwt
{
	public class ApiRole : IdentityRole
	{
		public ApiRole() : base() { }
		public ApiRole(string name) : base(name)
		{
			Id = name;
		}


		public const string Moderator = "moderator";
		public const string Wargaming = "wargaming";
		public const string Admin = "admin";
	}
}
