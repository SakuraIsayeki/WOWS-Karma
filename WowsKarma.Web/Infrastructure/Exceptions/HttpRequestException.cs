using System.Net;
using System.Net.Http;

namespace WowsKarma.Web.Infrastructure.Exceptions;

public class HttpRequestException : ApplicationException
{
	public HttpStatusCode ErrorStatusCode { get; }
	
	public HttpRequestException(HttpResponseMessage response, Exception e = null)
		: base($"HTTP Response returned Status Code {response.StatusCode} ({response.ReasonPhrase}) : \n{response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}", e)
	{
		ErrorStatusCode = response.StatusCode;
	}

	protected HttpRequestException(string message) : base(message) { }
}