using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Data.Repositories;

namespace WowsKarma.Api.Data
{
	public class UnitOfWork
	{
		private readonly ApiDbContext context;

		public IRepository<Player, uint> PlayerRepository { get; private set; }
		public EntityRepository<Post, Guid> PostRepository { get; private set; }


		public UnitOfWork(ApiDbContext context)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
			SetupRepositories();
		}

		public async Task<IDbContextTransaction> NewTransactionAsync() => await context.Database.BeginTransactionAsync();
		public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();

		private void SetupRepositories()
		{
			PlayerRepository = new EntityRepository<Player, uint>(context);
			PostRepository = new EntityRepository<Post, Guid>(context);
		}
	}
}
