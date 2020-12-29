using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowsKarma.Api.Services.Authentication
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class AccessKeyAttribute : Attribute, IAsyncAuthorizationFilter
	{
		private const string AccessKeyHeaderName = "Access-Key";
		private static readonly string apiKeysLocation = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "access-keys.txt";

		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			// "AllowAnonymous" skips all authorization
			if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
			{
				return;
			}

			if (!context.HttpContext.Request.Headers.TryGetValue(AccessKeyHeaderName, out StringValues extractedApiKey))
			{
				context.Result = new UnauthorizedResult();
			} 

			string[] apiKeys = await File.ReadAllLinesAsync(apiKeysLocation, Encoding.ASCII);

			if (!apiKeys.ToList().Contains(extractedApiKey))
			{
				context.Result = new UnauthorizedResult();
			}
		}
	}
}
