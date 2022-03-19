using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WowsKarma.Web.Infrastructure.Exceptions;

namespace WowsKarma.Web.Services.Api;

public abstract class ApiClientBase : IDisposable
{
	private bool disposedValue;

	protected HttpClient Client { get; init; }
	protected IHttpContextAccessor HttpContextAccessor { get; init; }
	protected static JsonSerializerOptions SerializerOptions => Common.Utilities.ApiSerializerOptions;

	public ApiClientBase(HttpClient client, IHttpContextAccessor contextAccessor)
	{
		Client = client;
		Client.DefaultRequestHeaders.Authorization = contextAccessor.HttpContext.GenerateAuthenticationHeader();

		HttpContextAccessor = contextAccessor;
	}


	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				Client.Dispose();
			}

			disposedValue = true;
		}
	}
	
	protected static async Task EnsureSuccessfulResponseAsync(HttpResponseMessage response)
	{
		if (!response.IsSuccessStatusCode)
		{
			ProblemDetails apiError = null;

			try
			{
				apiError = await response.Content.ReadFromJsonAsync<ProblemDetails>(SerializerOptions);
			}
			catch
			{
				await EnsureSuccessfulResponseAsync(response);
			}

			throw new ApiErrorResponseException(apiError);
		} 
	}
}