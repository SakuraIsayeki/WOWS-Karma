using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Api.Data.Models
{
	public interface IDataModel<T> where T : struct
	{
		public T Id { get; init; }

		public virtual bool IdEquals<TId>(IDataModel<TId> entity1, IDataModel<TId> entity2) where TId : struct => entity1.Id.Equals(entity2.Id);
	}
}
