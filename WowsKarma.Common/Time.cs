using System.Globalization;

namespace WowsKarma.Common;

public static class Time
{
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
	/// Returns a DateTime based on provided Unix timestamp
	/// </summary>
	/// <param name="timestamp">UNIX/POSIX timestamp</param>
	/// <returns>DateTime</returns>
	public static DateTime ParseUnixTimestamp(long timestamp) => DateTime.UnixEpoch.AddSeconds(timestamp).ToLocalTime();
}
