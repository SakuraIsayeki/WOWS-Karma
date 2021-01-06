using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace WowsKarma.Api.Utilities
{
	public static class DataUtilities
	{
		public static async Task<IDictionary<TId, TModel>> FindManyAsync<TId, TModel>(this DbSet<TModel> entities, TId[] ids)
		where TId : struct
		where TModel : class
		{
			Dictionary<TId, TModel> results = new();
			foreach (TId id in ids)
			{
				results.Add(id, await entities.FindAsync());
			}

			return results;
		}
	}
}
