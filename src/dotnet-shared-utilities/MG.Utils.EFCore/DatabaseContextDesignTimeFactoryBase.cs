using MG.Utils.Abstract.NonNullableObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MG.Utils.EFCore
{
    public abstract class DatabaseContextDesignTimeFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : DbContext
    {
        protected abstract NonNullableString ConnectionString { get; }

        protected IConfigurationRoot Configuration { get; }

        protected DatabaseContextDesignTimeFactoryBase(NonNullableString basePath)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json")
                .AddEnvironmentVariables()
                .Build();
        }

        public TContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            ShowConnectionString();
            return CreateContext(builder);
        }

        protected abstract TContext CreateContext(DbContextOptionsBuilder<TContext> builder);

        protected virtual void ShowConnectionString()
        {
            System.Console.WriteLine(ConnectionString);
        }
    }
}