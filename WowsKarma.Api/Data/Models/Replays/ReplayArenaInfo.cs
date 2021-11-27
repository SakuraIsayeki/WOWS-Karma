using Wargaming.WebAPI.Models;

namespace WowsKarma.Api.Data.Models.Replays;

/*
 *	https://dev.azure.com/wows-monitor/_git/api?path=/wows-monitor.core/appmodels/arenainfo/Arenainfo.cs
 */

public record ReplayArenaInfo
{
	public short MapId { get; set; }
	public int PlayerId { get; set; }

	public MatchGroup MatchGroup { get; set; } = MatchGroup.PVP;

	public List<Ship> Vehicles { get; set; }
	public DateTime DateTime { get; set; }
	public string Token { get; set; }
	public Region Region { get; set; }


	//public int Duration { get; set; }
	//public string GameLogic { get; set; }
	//public string Name { get; set; }
	//public string Scenario { get; set; }
	//public string ClientVersionFromXml { get; set; }
	//public int GameMode { get; set; }
	//public string ClientVersionFromExe { get; set; }
	//public string MapDisplayName { get; set; }
	//[JsonProperty("playerID")]
	//public int PlayersPerTeam { get; set; }
	//public string MapName { get; set; }
	//public string PlayerName { get; set; }
	//public int ScenarioConfigId { get; set; }
	//public int TeamsCount { get; set; }
	//public string Logic { get; set; }
	//public string PlayerVehicle { get; set; }
}

public record Ship : IHasRelation
{
	public int Id { get; set; }

	public long ShipId { get; set; }

	public Relation Relation { get; set; }

	public string Name { get; set; }
}

