using MG.Utils.Abstract.NonNullableObjects;
using Microsoft.Extensions.DependencyInjection;

namespace MG.Utils.AspNetCore.Redis
{
    public static class RedisConfig
    {
        public static void AddDistributedRedisCache(this IServiceCollection services, NonNullableString redisConnectionString)
        {
            services.AddScoped<ICache, ApplicationCache>();

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "master";
            });
        }
    }
}