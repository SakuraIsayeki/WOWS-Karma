using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

namespace WowsKarma.AppHost.Extensions;

/// <summary>
/// Adapts AppHost parameter configuration into reusable secrets, optional values, and indexed
/// environment variables understood by .NET configuration binding.
/// </summary>
internal static class ParameterExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        /// Adds a secret parameter with a generated default that is persisted for reuse across runs.
        /// </summary>
        /// <param name="name">The Aspire parameter name, configurable through <c>Parameters:{name}</c>.</param>
        /// <param name="length">The minimum length of the generated default, which excludes special characters.</param>
        /// <returns>The secret parameter resource builder.</returns>
        /// <remarks>
        /// An explicitly configured value takes precedence over generation. The length and character
        /// settings apply to generated defaults; they do not validate supplied values.
        /// </remarks>
        public IResourceBuilder<ParameterResource> AddGeneratedSecret(string name, int length) =>
            builder.AddParameter(name, new GenerateParameterDefault { MinLength = length, Special = false },
                secret: true, persist: true);

        /// <summary>
        /// Registers an optional parameter and returns it only when its configuration value is nonblank.
        /// </summary>
        /// <param name="name">The parameter name whose value is read from <c>Parameters:{name}</c>.</param>
        /// <param name="secret">Whether Aspire should treat the parameter value as a secret.</param>
        /// <returns>The parameter builder when configured; otherwise, <see langword="null"/>.</returns>
        /// <remarks>
        /// The parameter remains registered as optional even when this method returns null, allowing
        /// callers to select a local resource or another fallback without requiring a value.
        /// </remarks>
        public IResourceBuilder<ParameterResource>? AddOptionalParameter(string name, bool secret = false)
        {
            var parameter = builder.AddParameter(name, secret: secret).AsOptional();
            return string.IsNullOrWhiteSpace(builder.Configuration[$"Parameters:{name}"]) ? null : parameter;
        }
    }

    /// <summary>
    /// Exposes the children of a parameter section as individually tracked Aspire parameters and
    /// consecutive indexed environment variables.
    /// </summary>
    /// <typeparam name="T">The resource type that accepts environment variables.</typeparam>
    /// <param name="resource">The resource that receives the indexed configuration values.</param>
    /// <param name="name">The configuration section name beneath <c>Parameters</c>.</param>
    /// <param name="environmentPrefix">The destination configuration path, using double underscores as separators.</param>
    /// <param name="secret">Whether each generated parameter resource should be treated as a secret.</param>
    /// <returns>The original resource builder for further configuration.</returns>
    /// <remarks>
    /// Children are enumerated in configuration order and renumbered from zero. Each value is read
    /// from its original configuration path through a parameter named <c>{name}-{index}</c> and
    /// exposed as <c>{environmentPrefix}__{index}</c>. An absent or empty section adds no variables.
    /// </remarks>
    public static IResourceBuilder<T> WithArrayParameters<T>(this IResourceBuilder<T> resource,
        string name, string environmentPrefix, bool secret)
        where T : IResourceWithEnvironment
    {
        var builder = resource.ApplicationBuilder;
        var values = builder.Configuration.GetSection($"Parameters:{name}").GetChildren();
        foreach (var (value, index) in values.Select((value, index) => (value, index)))
        {
            var parameter = builder.AddParameterFromConfiguration($"{name}-{index}", value.Path, secret);
            resource.WithEnvironment($"{environmentPrefix}__{index}", parameter);
        }

        return resource;
    }
}
