using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WowsKarma.Web.Services;

public abstract class HttpServiceBase : IDisposable
{
	private bool disposedValue;

	protected HttpClient Client { get; init; }
	protected IHttpContextAccessor HttpContextAccessor { get; init; }
	protected JsonSerializerOptions SerializerOptions => WowsKarma.Common.Utilities.ApiSerializerOptions;
	
	public HttpServiceBase(IHttpClientFactory httpClientFactory, string httpClientName, IHttpContextAccessor contextAccessor)
	{
		Client = httpClientName is null ? httpClientFactory.CreateClient() : httpClientFactory.CreateClient(httpClientName);
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
}