using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs;
using WowsKarma.Web.Services.Authentication;

namespace WowsKarma.Web.Shared.Components
{
	public abstract class ComponentBaseAuth : ComponentBase
	{
		[Inject] protected IHttpContextAccessor HttpContextAccessor { get; set; }
		[Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }

		protected AccountListingDTO CurrentUser { get; private set; }
		protected ClaimsPrincipal ClaimsPrincipal { get; private set; }
		protected string CurrentToken { get; private set; }

		protected async override Task OnParametersSetAsync()
		{
			AuthenticationState authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
			CurrentUser = authenticationState.User.ToAccountListing();
			CurrentToken = HttpContextAccessor.HttpContext.Request.Cookies[ApiTokenAuthenticationHandler.CookieName];
		}

		protected static JwtSecurityToken ParseToken(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);
	}
}
