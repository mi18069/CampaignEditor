using Database.DTOs.ChannelDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ChannelExtensions
    {
        public static void AddChannelExtensions(this IServiceCollection services)
        {
            services.AddScoped<IChannelRepository, ChannelRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<ChannelDTO, Channel>().ReverseMap();
            });

        }
    }
}
