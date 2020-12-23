using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Web.Models.ViewModels
{
	public record SearchViewModel<TResult>
	{
		public string Search { get; init; }

		public IEnumerable<TResult> Results { get; init; }
	}
}
