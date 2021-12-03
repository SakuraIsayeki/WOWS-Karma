namespace WowsKarma.Api.Infrastructure.Exceptions;

[Serializable]
public class InvalidReplayException : ApplicationException
{
	public InvalidReplayException(Exception e) : base("Exception thrown when processing replay.", e) { }
	public InvalidReplayException(string message, Exception e = null) : base(message, e) { }

}
