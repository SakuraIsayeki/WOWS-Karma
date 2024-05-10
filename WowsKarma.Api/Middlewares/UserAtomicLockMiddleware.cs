using System.Collections.Concurrent;
using WowsKarma.Api.Infrastructure.Attributes;
using WowsKarma.Common;

namespace WowsKarma.Api.Middlewares;

/// <summary>
/// Provides a middleware to lock a given action to a given user.
/// </summary>
public class UserAtomicLockMiddleware : IMiddleware
{
    private static readonly ConcurrentDictionary<Tuple<string, uint>, object> Locks = new();
    private static object ConcurrencyLock = new();

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        // Get the current user and endpoint
        uint? uid = ctx.User.ToAccountListing()?.Id;
        PathString path = ctx.Request.Path;

        if (uid is null)
        {
            // Pass through.
            await next(ctx);
            return;
        }

        if (ctx.GetEndpoint()?.Metadata.GetMetadata<UserAtomicLockAttribute>() is null)
        {
            // Pass through.
            await next(ctx);
            return;
        }

        bool lockExists;
        // lock (ConcurrencyLock)
        // {
             lockExists = Locks.TryGetValue(new(path, uid.Value), out _) || !Locks.TryAdd(new(path, uid.Value), new());
        // }
        
        // Get or try to add the lock object.
        if (lockExists)
        {
            // Lock is already taken.
            ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }
        
        // Hold the lock for the request's duration.
        try
        {
            await next(ctx);
        }
        finally
        {
            // lock (ConcurrencyLock)
            // {
                // Release the lock.
                Locks.TryRemove(new(path, uid.Value), out _);
            // }
        }
    }
}