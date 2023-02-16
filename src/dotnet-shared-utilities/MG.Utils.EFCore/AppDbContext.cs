using System;
using System.Threading;
using System.Threading.Tasks;
using MG.Utils.Abstract.Dates.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MG.Utils.EFCore
{
    public abstract class AppDbContextBase<TContext> : DbContext
        where TContext : DbContext
    {
        protected AppDbContextBase(DbContextOptions<TContext> options)
            : base(options)
        {
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(TContext).Assembly);
        }

        protected virtual void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries<IHasDates>();
            var currentDateTime = DateTimeOffset.Now;

            foreach (EntityEntry<IHasDates> entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.Entity.OnUpdate(currentDateTime);
                        break;

                    case EntityState.Added:
                        entry.Entity.OnCreate(currentDateTime);
                        break;
                }
            }
        }
    }
}