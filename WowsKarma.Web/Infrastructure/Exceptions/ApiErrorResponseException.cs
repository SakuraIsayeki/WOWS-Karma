using Microsoft.AspNetCore.Mvc;

namespace WowsKarma.Web.Infrastructure.Exceptions;

[Serializable]
public class ApiErrorResponseException : Exception
{
	public ProblemDetails ApiError { get; }

	public ApiErrorResponseException(ProblemDetails apiError, Exception inner = null) : base("API responded with an error.", inner)
	{
		ApiError = apiError;
	}

	public override string StackTrace => ApiError.Type?.EndsWith("Exception") is true
		? ApiError.Detail
		: base.StackTrace;

	public override string Message => $"{ApiError.Type}: {ApiError.Title}";
}