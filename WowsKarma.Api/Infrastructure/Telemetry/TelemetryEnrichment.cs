using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using WowsKarma.Common;

namespace WowsKarma.Api.Infrastructure.Telemetry;

public class TelemetryEnrichment : TelemetryInitializerBase
{
	public TelemetryEnrichment(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }


	protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry, ITelemetry telemetry)
	{
		// API Region
		telemetry.Context.GlobalProperties["api-region"] ??= Startup.ApiRegion.ToRegionString();

		// User ID
		AccountListingDTO userAccount = platformContext.User?.ToAccountListing();
		telemetry.Context.User.AuthenticatedUserId = userAccount?.Id.ToString() ?? string.Empty;

		// IP Address
		if (telemetry is ISupportProperties propTelemetry && !propTelemetry.Properties.ContainsKey("client-ip"))
		{
			string clientIPValue = telemetry.Context.Location.Ip;
			propTelemetry.Properties.Add("client-ip", clientIPValue);
		}
	}
}
