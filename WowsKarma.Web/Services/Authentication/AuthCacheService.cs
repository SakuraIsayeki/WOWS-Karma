using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.Extensions.Hosting;
using WowsKarma.Common.Hubs;

namespace WowsKarma.Web.Services.Authentication;

public class AuthCacheService : IHostedService, IAsyncDisposable
{
	private readonly IMemoryCache _cache;
	private readonly IConfiguration _configuration;
	private readonly ILogger<AuthCacheService> _logger;
	private readonly HubConnection _hub;
	private bool _disposedValue;

	public AuthCacheService(IMemoryCache cache, IConfiguration configuration, ILogger<AuthCacheService> logger)
	{
		_cache = cache;
		_configuration = configuration;
		_logger = logger;
		
		_hub = new HubConnectionBuilder()
			.WithAutomaticReconnect()
			.WithUrl(_configuration[$"Api:{Utilities.CurrentRegion}:AuthHub"])
			.Build();
		
		HookHandlers();
	}

	public async Task StartAsync(CancellationToken ct)
	{
		try
		{
			await _hub.StartAsync(ct);
			_logger.LogInformation("Started Authentication Cache Hub Listener. Connection ID: {ConnectionId}", _hub.ConnectionId);
		}
		catch (Exception e)
		{
			_logger.LogCritical(e, "Exception caught while connecting to the Authentication Hub.");
		}
	}

	public async Task StopAsync(CancellationToken ct)
	{
		await _hub.StopAsync(ct);
		_logger.LogInformation("Stopped Authentication Cache Hub Listener.");
	}
	
	protected void HookHandlers() => _hub.On<uint>(nameof(IAuthHubPush.SeedTokenInvalidated), EvictUserSeedToken);

	public void NewUserSeedToken(JwtSecurityToken token)
	{
		uint userId = Convert.ToUInt32(token.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
		Guid seedToken = Guid.Parse(token.Claims.FirstOrDefault(c => c.Type is "seed")?.Value ?? throw new InvalidOperationException());

		_cache.Set(userId, seedToken, token.ValidTo);
		_logger.LogInformation("Added seed token for user {UserId} to Auth Cache. Valid until: {Expiration}", userId, token.ValidTo);
	}

	public bool IsValidToken(JwtSecurityToken token)
	{
		bool valid = token.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier)?.Value is { } userIdStr 
			&& token.Claims.FirstOrDefault(c => c.Type is "seed")?.Value is { } seedStr
			&& HasSeedToken(Convert.ToUInt32(userIdStr), Guid.Parse(seedStr));

		if (valid)
		{
			_logger.LogDebug("Auth token validated from cache.");
		}

		return valid;
	}

	public bool HasSeedToken(uint userId, Guid token) => _cache.TryGetValue(userId, out Guid currentToken) && token == currentToken;

	public void EvictUserSeedToken(uint userId)
	{
		_cache.Remove(userId);
		_logger.LogInformation("Eviction ordered for user {UserId} from Auth Cache.", userId);
	}

	#region Dispose

	protected virtual async ValueTask DisposeAsync(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
				await _hub.DisposeAsync();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_disposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~AuthCacheService()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public async ValueTask DisposeAsync()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		await DisposeAsync(true);
		GC.SuppressFinalize(this);
	}

	#endregion // Dispose
}
