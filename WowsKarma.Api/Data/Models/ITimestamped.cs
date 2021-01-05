using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Api.Data.Models
{
	public interface ITimestamped
	{
		public DateTime CreatedAt { get; init; }
		public DateTime UpdatedAt { get; set; }
	}
}
