using Microsoft.AspNetCore.Mvc;

namespace WowsKarma.Api.Controllers
{
	/// <summary>
	/// Provides status endpoints for controlling API lifetime.
	/// </summary>
	[ApiController, Route("api/[controller]")]
	public class StatusController : Controller
	{
		/// <summary>
		/// Provides a HTTP ping endpoint.
		/// </summary>
		/// <response code="200">Service is healthy.</response>
		[HttpGet, ProducesResponseType(200)]
		public IActionResult Status() => Ok();
	}
}
