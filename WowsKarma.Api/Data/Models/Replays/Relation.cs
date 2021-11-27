namespace WowsKarma.Api.Data.Models.Replays;

public enum Relation
{
	Self = 0,
	Friendly = 1,
	Enemy = 2
}

public interface IHasRelation
{
	Relation Relation { get; set; }
}
