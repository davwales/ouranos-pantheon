using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;

namespace Ouranos.Pantheon.Tests.Utils.Extensions;

public static class DbContextExtensions
{
    public static TContext Mock<TContext>(string? databaseName = null) where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var instance = Activator.CreateInstance(typeof(TContext), options);
        Guard.Against.Null(instance);

        return (TContext)instance;
    }

    public static async Task SeedData<TContext, TEntity>(this TContext dbContext, params TEntity[] entities)
        where TContext : DbContext
        where TEntity : class
    {
        await dbContext.Set<TEntity>().AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }
}