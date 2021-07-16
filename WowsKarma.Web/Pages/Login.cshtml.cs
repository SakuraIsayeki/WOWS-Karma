using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WowsKarma.Web.Pages
{
	public class LoginModel : PageModel
	{
		public async Task OnGet(string redirectUri) => await HttpContext.ChallengeAsync(new AuthenticationProperties { RedirectUri = redirectUri });
	}
}
