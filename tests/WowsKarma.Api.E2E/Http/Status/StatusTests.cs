using System.Net;
using System.Reflection;
using System.Text.Json;
using TUnit.Core;
using WowsKarma.Api.E2E.Support;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Http.Status;

/// <summary>Readiness and published API documentation.</summary>
[ClassDataSource<ApiFixture>(Shared = SharedType.PerTestSession)]
[Category("Status")]
public sealed class StatusTests(ApiFixture fixture)
{
    [Test]
    [DisplayName("The running API responds to its health check and exposes the post contract in OpenAPI.")]
    public async Task ExposesStatusAndOpenApi()
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync("/api/status"), HttpStatusCode.OK);
        var version = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0];
        using var swagger = await client.GetAsync($"/swagger/{version}/swagger.json");
        swagger.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await swagger.Content.ReadAsStringAsync());
        Check(json.RootElement.GetProperty("paths").TryGetProperty("/api/Post", out _), "Post contract absent from OpenAPI");
    }
}
