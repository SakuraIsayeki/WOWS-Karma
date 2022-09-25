using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using WowsKarma.Api.Infrastructure.Data;

namespace WowsKarma.Api.Utilities;

#nullable enable

/// <summary>
/// Provides HTTP extensions for request/response interaction.
/// </summary>
public static class HttpExtensions
{
	/// <summary>
	/// Adds pagination headers to the response.
	/// </summary>
	/// <param name="response">The response to add the headers to.</param>
	/// <param name="pageMeta">The page metadata.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddPaginationHeaders(this HttpResponse response, PageMeta pageMeta)
	{
		response.Headers.Add("Content-Page-Current", pageMeta.CurrentPage.ToString());
		response.Headers.Add("Content-Page-Size", pageMeta.PageSize.ToString());
		response.Headers.Add("Content-Page-Total", pageMeta.TotalPages.ToString());
		response.Headers.Add("Content-Items-Total", pageMeta.ItemsCount.ToString());
	}
	
	/// <summary>
	/// Sets up CORS configuration for pagination headers.
	/// </summary>
	/// <param name="builder">The builder to add the configuration to.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WithExposedPaginationHeaders(this CorsPolicyBuilder builder)
	{
		builder.WithExposedHeaders("Content-Page-Current", "Content-Page-Size", "Content-Page-Total", "Content-Items-Total");
	}
}