using Aspire.Hosting.ApplicationModel;

namespace WowsKarma.AppHost.Extensions;

/// <summary>
/// Bridges Aspire's HTTPS certificate configuration to the Angular development server launcher.
/// </summary>
internal static class WebExtensions
{
    /// <summary>
    /// Supplies the HTTPS development certificate and private key paths expected by the web launcher.
    /// </summary>
    /// <typeparam name="T">The web resource type supporting environment variables and arguments.</typeparam>
    /// <param name="web">The web resource running <c>tools/aspire-serve.cjs</c>.</param>
    /// <returns>The web resource builder with developer certificate support configured.</returns>
    /// <remarks>
    /// The certificate callback sets <c>WOWSKARMA_WEB_CERT</c> and <c>WOWSKARMA_WEB_KEY</c> after
    /// Aspire determines the certificate paths. The launcher passes them to Angular's SSL options.
    /// This helper is intended for the development server and exposes the private key path to it.
    /// </remarks>
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
