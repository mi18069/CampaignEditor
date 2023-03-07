
using Database.DTOs.SeasonalityDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static  class SeasonalityExtensions
    {
        public static void AddSeasonalityExtensions(this IServiceCollection services)
        {
            services.AddScoped<ISeasonalityRepository, SeasonalityRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<SeasonalityDTO, Seasonality>().ReverseMap();
            });

        }
    }
}
