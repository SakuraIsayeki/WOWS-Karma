using System.Net;

namespace WowsKarma.Api.E2E.Support;

/// <summary>Adds response diagnostics to assertions against the out-of-process API.</summary>
internal static class ApiAssert
{
    internal static async Task Expect(HttpResponseMessage response, HttpStatusCode expected)
    {
        using (response)
        {
            if (response.StatusCode != expected)
                throw new InvalidOperationException($"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}: expected {(int)expected}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
    }

    internal static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
