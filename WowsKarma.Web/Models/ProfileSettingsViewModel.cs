using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Web.Models
{
	public record ProfileSettingsViewModel
	{
		public uint Id { get; init; }

		public bool OptedOut { get; set; }
	}
}
