using Database.DTOs.SectableChannels;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class SectableChannelsExtensions
    {
        public static void AddSectableChannelsExtensions(this IServiceCollection services)
        {
            services.AddScoped<ISectableChannelsRepository, SectableChannelsRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<SectableChannelsDTO, SectableChannels>().ReverseMap();
            });

        }
    }
}
