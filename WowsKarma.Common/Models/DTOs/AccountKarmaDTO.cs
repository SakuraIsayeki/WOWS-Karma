using System.Collections.Generic;

namespace WowsKarma.Common.Models.DTOs
{
	// Used by WOWS Monitor
	public record AccountFullKarmaDTO(uint Id, int Karma, int Performance, int Teamplay, int Courtesy);

	public record AccountKarmaDTO(uint Id, int Karma)
	{
		public static implicit operator AccountKarmaDTO(AccountFullKarmaDTO value) => new(value.Id, value.Karma);

		public static Dictionary<uint, int> ToDictionary(IEnumerable<AccountKarmaDTO> values)
		{
			Dictionary<uint, int> pairs = new();
			foreach (AccountKarmaDTO value in values)
			{
				pairs.Add(value.Id, value.Karma);
			}

			return pairs;
		}
	}
}
