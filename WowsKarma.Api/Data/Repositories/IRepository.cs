using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;

namespace WowsKarma.Api.Data
{
	public interface IRepository<TEntity, TId> 
		where TEntity : class, IDataModel<TId>, new() 
		where TId : struct
	{
		public IQueryable<TEntity> GetAll();
		public Task<TEntity> Get(TId id);
		
		public Task CreateAsync(TEntity entity);
		public Task UpdateAsync(TEntity entity);
		public Task DeleteAsync(TEntity entity);
	}
}
