using Database.DTOs.CobrandDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class CobrandExtensions
    {
        public static void AddCobrandExtensions(this IServiceCollection services)
        {
            services.AddScoped<ICobrandRepository, CobrandRepository>();
            services.AddAutoMapper(configuration =>
            {
                configuration.CreateMap<CobrandDTO, Cobrand>().ReverseMap();
            });
        }
    }
}
