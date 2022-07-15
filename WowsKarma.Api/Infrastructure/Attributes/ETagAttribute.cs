namespace WowsKarma.Api.Infrastructure.Attributes;

/// <summary>
/// Attribute for controlling ETag generation for a given endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ETagAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ETagAttribute"/> class.
	/// </summary>
	/// <param name="enabled">ETag generation is enabled if set to <c>true</c></param>
	public ETagAttribute(bool enabled = true)
	{
		Enabled = enabled;
	}

	public bool Enabled { get; }
}
