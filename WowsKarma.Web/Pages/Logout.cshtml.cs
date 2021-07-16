using System.Threading.Tasks;
using AngleSharp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WowsKarma.Web.Services.Authentication;

namespace WowsKarma.Web.Pages
{
	public class LogoutModel : PageModel
	{
		public IActionResult OnGet()
		{
			Response.Cookies.Delete(ApiTokenAuthenticationHandler.CookieName);
			return Redirect("/");
		}
	}
}
