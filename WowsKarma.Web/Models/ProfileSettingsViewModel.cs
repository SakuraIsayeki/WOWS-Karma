using System;



namespace WowsKarma.Web.Models
{
	public record ProfileSettingsViewModel
	{
		public uint Id { get; init; }

		public bool OptedOut { get; set; }

		public DateTime OptOutChanged { get; init; }
	}
}
