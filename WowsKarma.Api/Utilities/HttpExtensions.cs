using Microsoft.AspNetCore.Cors.Infrastructure;
using WowsKarma.Api.Infrastructure.Data;

namespace WowsKarma.Api.Utilities;

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
	public static void AddPaginationHeaders(this HttpResponse response, PageMeta pageMeta)
	{
		response.Headers.Append("Content-Page-Current", pageMeta.CurrentPage.ToString());
		response.Headers.Append("Content-Page-Size", pageMeta.PageSize.ToString());
		response.Headers.Append("Content-Page-Total", pageMeta.TotalPages.ToString());
		response.Headers.Append("Content-Items-Total", pageMeta.ItemsCount.ToString());
	}
	
	/// <summary>
	/// Sets up CORS configuration for pagination headers.
	/// </summary>
	/// <param name="builder">The builder to add the configuration to.</param>
	public static void WithExposedPaginationHeaders(this CorsPolicyBuilder builder)
	{
		builder.WithExposedHeaders("Content-Page-Current", "Content-Page-Size", "Content-Page-Total", "Content-Items-Total");
	}
}