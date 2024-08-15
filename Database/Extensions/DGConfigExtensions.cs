using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class DGConfigExtensions
    {
        public static void AddDGConfigExtensions(this IServiceCollection services)
        {
            services.AddScoped<IDGConfigRepository, DGConfigRepository>();

        }
    }
}
