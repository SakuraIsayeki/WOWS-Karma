using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace WowsKarma.Api.Infrastructure.Telemetry;

public sealed class HubTelemetryFilter : ITelemetryProcessor
{
	private ITelemetryProcessor Next { get; set; }

	public HubTelemetryFilter(ITelemetryProcessor next)
	{
		Next = next;
	}

	public void Process(ITelemetry item)
	{
		if (item is RequestTelemetry { Name: not null } request && request.Name.Contains("hub"))
		{
			return;
		}

		// Send everything else:
		Next.Process(item);
	}
}
