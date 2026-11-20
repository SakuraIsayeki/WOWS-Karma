using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

namespace WowsKarma.AppHost.Extensions;

internal static class ParameterExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public IResourceBuilder<ParameterResource> AddGeneratedSecret(string name, int length) =>
            builder.AddParameter(name, new GenerateParameterDefault { MinLength = length, Special = false },
                secret: true, persist: true);

        public IResourceBuilder<ParameterResource>? AddOptionalParameter(string name, bool secret = false)
        {
            var parameter = builder.AddParameter(name, secret: secret).AsOptional();
            return string.IsNullOrWhiteSpace(builder.Configuration[$"Parameters:{name}"]) ? null : parameter;
        }
    }

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
