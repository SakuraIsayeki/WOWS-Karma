using System.Text.Json;

namespace WowsKarma.Api.Minimap.Client.Infrastructure.Json;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
	public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();

	public override string ConvertName(string name)
	{
		// Conversion to other naming conventaion goes here. Like SnakeCase, KebabCase etc.
		return ToSnakeCase(name);
	}
	
	public static string ToSnakeCase(string str)
	{
		if (string.IsNullOrEmpty(str)) return str;

		Span<char> span = stackalloc char[str.Length * 2];
		int spanIndex = 0;

		for (int strIndex = 0; strIndex < str.Length; strIndex++)
		{
			char currentChar = str[strIndex];
			if (strIndex > 0 && char.IsUpper(currentChar))
			{
				span[spanIndex++] = '_';
			}

			span[spanIndex++] = char.ToLower(currentChar);
		}

		return new(span[..spanIndex]);
	}}