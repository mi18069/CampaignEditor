using Database.DTOs.PricesDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class PricesExtensions
    {
        public static void AddPricesExtensions(this IServiceCollection services)
        {
            services.AddScoped<IPricesRepository, PricesRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<PricesDTO, Prices>().ReverseMap();
            });

        }
    }
}
