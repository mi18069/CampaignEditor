using Database.DTOs.TargetClassDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class TargetClassExtensions
    {
        public static void AddTargetClassExtensions(this IServiceCollection services)
        {
            services.AddScoped<ITargetClassRepository, TargetClassRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<TargetClassDTO, TargetClass>().ReverseMap();
            });
        }
    }
}
