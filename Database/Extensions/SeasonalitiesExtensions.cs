using Database.DTOs.SeasonalitiesDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class SeasonalitiesExtensions
    {
        public static void AddSeasonalitiesExtensions(this IServiceCollection services)
        {
            services.AddScoped<ISeasonalitiesRepository, SeasonalitiesRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<SeasonalitiesDTO, Seasonalities>().ReverseMap();
            });

        }
    }
}
