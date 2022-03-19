using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
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
		public IActionResult HandleError()
		{
			return StatusCode(500, HttpContext.Features.Get<IExceptionHandlerFeature>() is { } exceptionHandlerFeature
				? Problem(
					detail: exceptionHandlerFeature.Error.StackTrace, 
					instance: exceptionHandlerFeature.Path,
					title: exceptionHandlerFeature.Error.Message,
					statusCode: StatusCodes.Status500InternalServerError,
					type: exceptionHandlerFeature.Error.GetType().ToString())
				
				: Problem(statusCode: StatusCodes.Status500InternalServerError));
				
		}
	}
}
