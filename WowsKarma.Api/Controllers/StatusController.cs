using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

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
		
		[Route("/error"), ApiExplorerSettings(IgnoreApi = true)]
		public IActionResult HandleErrorDevelopment([FromServices] IHostEnvironment hostEnvironment)
		{
			if (!hostEnvironment.IsDevelopment())
			{
				return NotFound();
			}

			IExceptionHandlerFeature exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

			return Problem(detail: exceptionHandlerFeature?.Error.StackTrace, title: exceptionHandlerFeature?.Error.Message);
		}
}
