using Database.DTOs.ChannelGroupsDTO;
using Database.Repositories;
using Database.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ChannelGroupsExtensions
    {
        public static void AddChannelGroupsExtensions(this IServiceCollection services)
        {
            services.AddScoped<IChannelGroupsRepository, ChannelGroupsRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<ChannelGroupsDTO, ChannelGroups>().ReverseMap();
            });

        }
    }
}
