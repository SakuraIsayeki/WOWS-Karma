using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Web.Shared.Components
{
	public abstract class ComponentBaseAuth : ComponentBase
	{
		[Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }

		protected AccountListingDTO CurrentUser { get; set; }
		protected string CurrentToken { get; set; }

		protected async override Task OnParametersSetAsync()
		{
			AuthenticationState authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
			CurrentUser = authenticationState.User.ToAccountListing();
		}
	}
}
