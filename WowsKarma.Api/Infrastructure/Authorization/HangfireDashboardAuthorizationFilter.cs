using System.Security.Claims;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using WowsKarma.Common;

namespace WowsKarma.Api.Infrastructure.Authorization;

/// <summary>
/// Simple RBAC auth filter to check if the user has the Admin role, and grant access to the Hangfire dashboard if so.
/// Also grants readonly access to the Hangfire dashboard if the user has the CM role.
/// </summary>
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
	public static readonly HangfireDashboardAuthorizationFilter Instance = new();

	private HangfireDashboardAuthorizationFilter() { }

	public bool Authorize(DashboardContext context)
	{
		HttpContext httpContext = context.GetHttpContext();
		ClaimsPrincipal user = httpContext.User;

		return user.IsInRole(ApiRoles.Administrator) || user.IsInRole(ApiRoles.CM);
	}
	
	public static bool IsAccessReadOnly(DashboardContext context)
	{
		HttpContext httpContext = context.GetHttpContext();
		ClaimsPrincipal user = httpContext.User;

		return !user.IsInRole(ApiRoles.Administrator);
	}
}