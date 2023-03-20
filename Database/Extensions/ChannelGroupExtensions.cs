using Database.DTOs.ChannelGroupDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ChannelGroupExtensions
    {
        public static void AddChannelGroupExtensions(this IServiceCollection services)
        {
            services.AddScoped<IChannelGroupRepository, ChannelGroupRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<ChannelGroupDTO, ChannelGroup>().ReverseMap();
            });

        }
    }
}
