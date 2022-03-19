using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Web.Services;

public abstract class HttpServiceBase : IDisposable
{
	private bool disposedValue;

	protected HttpClient Client { get; init; }
	protected IHttpContextAccessor HttpContextAccessor { get; init; }
	protected static JsonSerializerOptions SerializerOptions => Common.Utilities.ApiSerializerOptions;
	
	public HttpServiceBase(HttpClient client, IHttpContextAccessor contextAccessor)
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
}