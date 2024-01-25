using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using WowsKarma.Api.Infrastructure.Attributes;

namespace WowsKarma.Api.Middlewares;

/// <summary>
/// Provides an ETag generation middleware.
/// </summary>
public sealed class ETagMiddleware
{
	private readonly RequestDelegate _next;

	public ETagMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		HttpResponse response = context.Response;
		await using Stream originalStream = response.Body;
		await using MemoryStream ms = new();

		response.Body = ms;

		await _next(context);

		if (IsEtagSupported(context))
		{
			string checksum = CalculateChecksum(ms);
			response.Headers[HeaderNames.ETag] = checksum;

			if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out StringValues etag) && checksum == etag)
			{
				response.StatusCode = StatusCodes.Status304NotModified;

				return;
			}
		}

		ms.Position = 0;
		await ms.CopyToAsync(originalStream);
	}

	private static bool IsEtagSupported(HttpContext context)
		// Check for the presence of the ETag attribute on the controller or its action.
		=> context.Features.Get<IEndpointFeature>() is { Endpoint: { } endpoint } && endpoint.Metadata.GetMetadata<ETagAttribute>() is not { Enabled: false }
			// 64KB is the limit for computing the checksum.
		&& context.Response is { StatusCode: StatusCodes.Status200OK, Body.Length: <= 64 * 1024 } response
		&& !response.Headers.ContainsKey(HeaderNames.ETag);

	private static string CalculateChecksum(Stream ms)
	{
		using SHA1 algo = SHA1.Create();

		ms.Position = 0;
		return $"\"{WebEncoders.Base64UrlEncode(algo.ComputeHash(ms))}\"";
	}
}

public static class ApplicationBuilderExtensions
{
	public static IApplicationBuilder UseETagger(this IApplicationBuilder app) => app.UseMiddleware<ETagMiddleware>();
}