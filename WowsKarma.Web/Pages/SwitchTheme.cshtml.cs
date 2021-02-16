using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static WowsKarma.Web.Shared.ThemeUtils;

namespace WowsKarma.Web.Pages
{
	public class SwitchThemeModel : PageModel
	{
		public IActionResult OnGet(string theme, string redirectUri)
		{
			Response.Cookies.Append(themeCookie, theme);
			return Redirect(redirectUri ?? "/");
		}
	}
}
