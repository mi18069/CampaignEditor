using Database.DTOs.ChannelCmpDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ChannelCmpExtensions
    {
        public static void AddChannelCmpExtensions(this IServiceCollection services)
        {
            services.AddScoped<IChannelCmpRepository, ChannelCmpRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<ChannelCmpDTO, ChannelCmp>().ReverseMap();
            });

        }
    }
}
