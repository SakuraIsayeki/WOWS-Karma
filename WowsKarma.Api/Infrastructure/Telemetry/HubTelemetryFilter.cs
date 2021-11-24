using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace WowsKarma.Api.Infrastructure.Telemetry;

public class HubTelemetryFilter : ITelemetryProcessor
{
	private ITelemetryProcessor Next { get; set; }

	public HubTelemetryFilter(ITelemetryProcessor next)
	{
		Next = next;
	}

	public void Process(ITelemetry item)
	{
		if (item is RequestTelemetry request and { Name: not null } && request.Name.Contains("hub"))
		{
			return;
		}

		// Send everything else:
		Next.Process(item);
	}
}
