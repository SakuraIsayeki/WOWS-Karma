using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WowsKarma.Api.Infrastructure.Attributes;

namespace WowsKarma.Api.Controllers;

/// <summary>
/// Provides status endpoints for controlling API lifetime.
/// </summary>
[ApiController, Route("api/[controller]"), ETag(false)]
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
		if (HttpContext.Features.Get<IExceptionHandlerFeature>() is { Error: not null } exceptionHandlerFeature)
		{
			Uri fullPath = new UriBuilder(Request.Scheme, Request.Host.Host, Request.Host.Port ?? 80, exceptionHandlerFeature.Path).Uri;
				
			return Problem(
				detail: exceptionHandlerFeature.Error.StackTrace,
				instance: fullPath.ToString(),
				title: exceptionHandlerFeature.Error.Message,
				statusCode: StatusCodes.Status500InternalServerError,
				type: exceptionHandlerFeature.Error.GetType().ToString()
			);
				
				
		}

		return Problem(statusCode: StatusCodes.Status500InternalServerError);
	}
}