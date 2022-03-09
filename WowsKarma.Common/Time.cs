using System.Globalization;

namespace WowsKarma.Common;

public static class Time
{
	/// <summary>
	/// Get an Instant object for Now, from the SystemClock.
	/// </summary>
	public static Instant Now => SystemClock.Instance.GetCurrentInstant();
	
	/// <summary>
	/// Converts a given DateTime into a Unix timestamp
	/// </summary>
	/// <param name="value">Any DateTime</param>
	/// <returns>The given DateTime in Unix timestamp format</returns>
	public static long ToUnixTimestamp(this DateTime value) => (long)value.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds;

	/// <summary>
	/// Gets a Unix timestamp representing the current moment
	/// </summary>
	/// <returns>Now expressed as a Unix timestamp</returns>
	public static long UnixTimestamp(this DateTime _) => (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

	/// <summary>
	/// Returns a local DateTime based on provided unix timestamp
	/// </summary>
	/// <param name="timestamp">Unix/posix timestamp</param>
	/// <returns>Local datetime</returns>
	public static DateTime ParseUnixTimestamp(long timestamp) => DateTime.UnixEpoch.AddSeconds(timestamp).ToLocalTime();
}
