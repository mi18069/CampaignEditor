using Database.DTOs.SpotDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class SpotExtensions
    {
        public static void AddSpotExtensions(this IServiceCollection services)
        {
            services.AddScoped<ISpotRepository, SpotRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<SpotDTO, Spot>().ReverseMap();
            });

        }
    }
}
