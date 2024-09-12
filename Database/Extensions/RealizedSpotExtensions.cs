using Database.DTOs.RealizedSpotDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class RealizedSpotExtensions
    {
        public static void AddRealizedSpotExtensions(this IServiceCollection services)
        {
            services.AddScoped<IRealizedSpotRepository, RealizedSpotRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<RealizedSpotDTO, RealizedSpot>().ReverseMap();
            });

        }
    }
}
