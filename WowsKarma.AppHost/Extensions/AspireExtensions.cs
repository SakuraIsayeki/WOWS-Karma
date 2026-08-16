namespace WowsKarma.AppHost.Extensions;

/// <summary>
/// Provides extension methods for configuring the Aspire Application Host.
/// </summary>
public static class AspireExtensions
{
    public static IReadOnlyList<KeyValuePair<string,string>> GetEnumOptions<TEnum>() => [..Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(e => new KeyValuePair<string, string>(e!.ToString()!, e.ToString()!))];
}