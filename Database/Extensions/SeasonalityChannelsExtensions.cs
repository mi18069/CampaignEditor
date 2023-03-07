using Database.DTOs.SeasonalityChannelsDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class SeasonalityChannelsExtensions
    {
        public static void AddSeasonalityChannelsExtensions(this IServiceCollection services)
        {
            services.AddScoped<ISeasonalityChannelsRepository, SeasonalityChannelsRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<SeasonalityChannelsDTO, SeasonalityChannels>().ReverseMap();
            });

        }
    }
}
