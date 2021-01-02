using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WowsKarma.Web.Pages
{
	public class LoginModel : PageModel
	{
		public IActionResult OnGet() => Challenge(new AuthenticationProperties { RedirectUri = "../" });
	}
}
