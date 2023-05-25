using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class DatabaseFunctionsExtensions
    {
        public static void AddDatabaseFunctionsExtensions(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseFunctionsRepository, DatabaseFunctionsRepository>();
            /*services.AddAutoMapper(configuration => {
                configuration.CreateMap<IDatabaseFunctionsRepository, DatabaseFunctionsRepository>().ReverseMap();
            });*/

        }
    }
}
