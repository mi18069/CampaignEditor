using Database.DTOs.PricelistChannels;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class PricelistChannelsExtensions
    {
        public static void AddPricelistChannelsExtensions(this IServiceCollection services)
        {
            services.AddScoped<IPricelistChannelsRepository, PricelistChannelsRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<PricelistChannelsDTO, PricelistChannels>().ReverseMap();
            });

        }
    }
}
