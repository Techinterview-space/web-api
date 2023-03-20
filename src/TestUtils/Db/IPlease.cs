using System.Threading.Tasks;
using Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace TestUtils.Db;

public interface IPlease<T>
    where T : class
{
    T Please();

    IPlease<T> AsPlease();

    async Task<T> PleaseAsync(DbContext context)
    {
        var entry = await context.AddEntityAsync(Please());
        await context.SaveChangesAsync();
        return entry;
    }
}