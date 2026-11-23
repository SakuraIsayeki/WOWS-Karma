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
    [DisplayName("The running API responds to its health check and exposes the post contract and bearer authentication in OpenAPI.")]
    public async Task ExposesStatusAndOpenApi()
    {
        using var client = fixture.Client();
        await Expect(await client.GetAsync("/api/status"), HttpStatusCode.OK);
        var version = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0];
        using var swagger = await client.GetAsync($"/swagger/{version}/swagger.json");
        swagger.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await swagger.Content.ReadAsStringAsync());
        Check(json.RootElement.GetProperty("paths").TryGetProperty("/api/Post", out _), "Post contract absent from OpenAPI");

        var bearer = json.RootElement.GetProperty("components").GetProperty("securitySchemes").GetProperty("jwt_auth");
        Check(bearer.GetProperty("type").GetString() is "http" && bearer.GetProperty("scheme").GetString() is "bearer",
            "JWT bearer security scheme absent from OpenAPI");
        Check(json.RootElement.GetProperty("security").EnumerateArray().Any(requirement =>
                requirement.TryGetProperty("jwt_auth", out var scopes) && scopes.GetArrayLength() == 0),
            "OpenAPI security requirement does not reference the JWT bearer scheme");
    }
}
