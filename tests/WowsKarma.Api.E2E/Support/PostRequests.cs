using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Api.E2E.Support.ApiAssert;

namespace WowsKarma.Api.E2E.Support;

/// <summary>Builds post payloads and submits them through the API's multipart contract.</summary>
internal static class PostRequests
{
    internal static PlayerPostDTO Draft(TestAccount author, TestAccount target) => new()
    {
        Author = new(author.Id, $"E2E_{author.Id}", null), Player = new(target.Id, $"E2E_{target.Id}", null),
        Title = "Baseline post", Content = "A baseline post with deterministic content.", Flairs = PostFlairs.PerformanceGood
    };

    internal static async Task<HttpResponseMessage> Create(HttpClient client, PlayerPostDTO draft, string query = "")
    {
        // The API binds post metadata from a named form field, with an optional replay part.
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(JsonSerializer.Serialize(draft, WowsKarma.Common.Utilities.ApiSerializerOptions)), "postDto");
        return await client.PostAsync("/api/post" + query, form);
    }

    internal static async Task<PlayerPostDTO> CreateOk(HttpClient client, PlayerPostDTO draft)
    {
        using var result = await Create(client, draft);
        Check(result.StatusCode == HttpStatusCode.Created, $"Create failed: {result.StatusCode} {await result.Content.ReadAsStringAsync()}");
        var post = (await result.Content.ReadFromJsonAsync<PlayerPostDTO>())!;
        Check(post.Id is { } id && result.Headers.Location?.ToString().Contains(id.ToString()) == true, "Missing creation Location/ID");
        return post;
    }
}
