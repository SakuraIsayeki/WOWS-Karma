namespace WowsKarma.Api.Infrastructure.Exceptions;

[Serializable]
public class CooldownException : Exception
{
	public CooldownException() { }

	public CooldownException(string cooldownType, object threshold, object actual)
		: base($"Cooldown not reached for {cooldownType} : Excepcted above {threshold}, currently {actual}")
	{
		Data["type"] = cooldownType;
		Data["threshold"] = threshold;
		Data["actual"] = actual;
	}

	public CooldownException(string message) : base(message) { }
	public CooldownException(string message, Exception inner) : base(message, inner) { }
}