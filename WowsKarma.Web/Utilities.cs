using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;



namespace WowsKarma.Web
{
	public static class Utilities
	{
		public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
		{
			PropertyNameCaseInsensitive = true,
			NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals
		};

		public static async Task<TResult> DeserializeFromHttpResponseAsync<TResult>(HttpResponseMessage response)
		{
			string json = await response.Content.ReadAsStringAsync();
			TResult result = JsonSerializer.Deserialize<TResult>(json, JsonSerializerOptions);
			return result;
		}
	}
}
