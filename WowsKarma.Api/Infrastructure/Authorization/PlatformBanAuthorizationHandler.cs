using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WowsKarma.Api.Data;

namespace WowsKarma.Api.Infrastructure.Authorization;

public sealed class PlatformBanAuthorizationHandler : AuthorizationHandler<PlatformBanRequirement>
{
	private readonly ILogger<PlatformBanAuthorizationHandler> _logger;
	private readonly ApiDbContext _dbContext;

	public PlatformBanAuthorizationHandler(ILogger<PlatformBanAuthorizationHandler> logger, ApiDbContext dbContext)
	{
		_logger = logger;
		_dbContext = dbContext;
	}
	
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PlatformBanRequirement requirement)
	{
		_logger.LogDebug("Evaluating platform ban requirement");
		
		// Parse the user's account ID from the auth context
		if (!uint.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out uint userId))
		{
			context.Fail(new(this, "User is not authenticated."));
		}
		
		// Get the user's enitity from the database, along with their platform bans
		Player? user = await _dbContext.Players.Include(p => p.PlatformBans).FirstOrDefaultAsync(p => p.Id == userId);

		// Calculate the currently active platform bans based on expiration time and reverted status
		bool activePlatformBan = user?.PlatformBans.Any(pb => !pb.Reverted
			&& (pb.BannedUntil is null // Indefinite ban
				|| pb.BannedUntil > DateTime.UtcNow)
		) ?? false; // Active temporary ban

		// Is the user banned?
		if (activePlatformBan || user is { PostsBanned: true })
		{
			context.Fail(new(this, "User is banned from the platform."));
			return;
		}
		
		// If we got here, the user is not banned
		context.Succeed(requirement);
	}
}