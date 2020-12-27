using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;

namespace WowsKarma.Api.Data.Repositories
{
	public class EntityRepository<TEntity, TId> : IRepository<TEntity, TId>
		where TEntity : class, IDataModel<TId>, new()
		where TId : struct
	{
		private readonly DbContext context;

		public EntityRepository(DbContext context)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
			_ = context.Set<TEntity>();	// Prodding Set<TEntity> to ensure it exists
		}

		internal DbSet<TEntity> GetDbSet() => context.Set<TEntity>();
		public IQueryable<TEntity> GetAll() => GetDbSet();

		public async Task<TEntity> GetAsync(TId id) => await GetDbSet().FindAsync(id);

		public Task CreateAsync(TEntity entity)
		{
			GetDbSet().Add(entity);
			return Task.CompletedTask;
		}

		public Task DeleteAsync(TEntity entity)
		{
			GetDbSet().Remove(entity);
			return Task.CompletedTask;
		}

		public Task UpdateAsync(TEntity entity)
		{
			GetDbSet().Update(entity);
			return Task.CompletedTask;
		}
	}
}
