using Aspire.Hosting.ApplicationModel;

namespace WowsKarma.AppHost.Extensions;

internal static class WebExtensions
{
    public static IResourceBuilder<T> WithAngularDevelopmentCertificate<T>(this IResourceBuilder<T> web)
        where T : IResourceWithEnvironment, IResourceWithArgs
    {
        #pragma warning disable ASPIRECERTIFICATES001 // Angular needs Aspire's HTTPS development key pair.
        return web.WithHttpsDeveloperCertificate()
            .WithHttpsCertificateConfiguration(context =>
            {
                context.EnvironmentVariables["WOWSKARMA_WEB_CERT"] = context.CertificatePath;
                context.EnvironmentVariables["WOWSKARMA_WEB_KEY"] = context.KeyPath;
                return Task.CompletedTask;
            });
        #pragma warning restore ASPIRECERTIFICATES001
    }
}
