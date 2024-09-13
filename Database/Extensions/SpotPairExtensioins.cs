using Database.DTOs.SpotPairDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class SpotPairExtensioins
    {
        public static void AddSpotPairExtensions(IServiceCollection services)
        {
            services.AddScoped<ISpotPairRepository, SpotPairRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<SpotPairDTO, SpotPair>().ReverseMap();
            });
        }
    }
}
