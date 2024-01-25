using System.Text.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using static WowsKarma.Api.Infrastructure.Resilience.PollyContextExtensions;

namespace WowsKarma.Api.Infrastructure.Resilience;

/// <summary>
/// Keys/Names of the different resilience pipelines.
/// </summary>
public static class ResiliencePipelines
{
    public const string PlayerClansUpdatePolicyName = "player-clans-update";
}

/// <summary>
/// Extension methods for adding resilience policies to the dependency injection container, using Polly.
/// </summary>
public static class ResiliencePipelineDependencyInjectionExtensions
{
    /// <summary>
    /// Adds resilience policies to the dependency injection container, using Polly.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the policies to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddResiliencePolicies(this IServiceCollection services)
    {
        services.AddResiliencePipeline<string, bool>(ResiliencePipelines.PlayerClansUpdatePolicyName, builder => builder
            .AddFallback(new()
            {
                ShouldHandle = static args => ValueTask.FromResult(args.Outcome.Exception is not null),
                
                FallbackAction = static args =>
                {
                    if (args.Outcome.Exception is not BrokenCircuitException or IsolatedCircuitException)
                    {
                        (args.Context.TryGetLogger(out ILogger? logger) ? logger : null)?
                            .LogWarning(args.Outcome.Exception, "Fallback triggered for {Policy} policy", ResiliencePipelines.PlayerClansUpdatePolicyName);
                    }
                    
                    return Outcome.FromResultAsValueTask(false);
                }
            })
            .AddCircuitBreaker(new()
            {
                ShouldHandle = static args => ValueTask.FromResult(args.Outcome switch
                {
                    { Exception: JsonException } => true,
                    _ => false
                }),
                
                OnClosed = static args =>
                {
                    (args.Context.TryGetLogger(out ILogger? logger) ? logger : null)?
                        .LogInformation("Circuit closed for {Policy} policy", ResiliencePipelines.PlayerClansUpdatePolicyName);
                    
                    return ValueTask.CompletedTask;
                },
                
                OnHalfOpened = static args =>
                {
                    (args.Context.TryGetLogger(out ILogger? logger) ? logger : null)?
                        .LogInformation("Circuit half-opened for {Policy} policy", ResiliencePipelines.PlayerClansUpdatePolicyName);
                    
                    return ValueTask.CompletedTask;
                },
                
                OnOpened = static args =>
                {
                    (args.Context.TryGetLogger(out ILogger? logger) ? logger : null)?
                        .LogWarning("Circuit opened for {Policy} policy", ResiliencePipelines.PlayerClansUpdatePolicyName);
                    
                    return ValueTask.CompletedTask;
                }
            })
            .AddRetry(new()
            {
                BackoffType = DelayBackoffType.Exponential, 
                MaxRetryAttempts = 4,
                ShouldHandle = static args => ValueTask.FromResult(args.Outcome switch
                {
                    { Exception: HttpRequestException } => true,
                    _ => false
                })
            })
        );

        return services;
    }
}