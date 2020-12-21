using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace WowsKarma.Web.Models
{
	public record PlayerListing
	{
		public uint Id { get; set; }

		public string Username { get; set; }
	}
}
