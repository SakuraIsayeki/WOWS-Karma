using System.Diagnostics.CodeAnalysis;
using Polly;

namespace WowsKarma.Api.Infrastructure.Resilience;

public static class PollyContextExtensions
{
    /// <summary>
    /// Defines the item names for context items.
    /// </summary>
    /// <seealso cref="ResilienceContext"/>
    public static class ContextItems
    {
        public static readonly ResiliencePropertyKey<ILogger> Logger = new("logger");
    }
    
    /// <summary>
    /// Gets the logger from the resilience context.
    /// </summary>
    /// <param name="context">The resilience context.</param>
    /// <param name="logger">The logger, if initially provided to the context and found.</param>
    /// <returns></returns>
    public static bool TryGetLogger(this ResilienceContext context, [NotNullWhen(true)] out ILogger? logger) 
        => context.Properties.TryGetValue(ContextItems.Logger, out logger);
}