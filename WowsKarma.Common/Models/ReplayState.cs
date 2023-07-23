namespace WowsKarma.Common.Models;

/// <summary>
/// Defines the state of a replay.
/// </summary>
public enum ReplayState : byte
{
	/// <summary>
	/// No replay associated.
	/// </summary>
	None = 0,
	
	/// <summary>
	/// Replay is being processed.
	/// </summary>
	Processing = 1,
	
	/// <summary>
	/// Replay has been processed, and is ready.
	/// </summary>
	Ready = 2
}