using Database.DTOs.PricelistDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class PricelistExtensions
    {
        public static void AddPricelistExtensions(this IServiceCollection services)
        {
            services.AddScoped<IPricelistRepository, PricelistRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<PricelistDTO, Pricelist>().ReverseMap();
            });

        }
    }
}
