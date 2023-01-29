using Database.DTOs.TargetValueDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;


namespace Database.Extensions
{
    public static class TargetValueExtensions
    {
        public static void AddTargetValueExtensions(this IServiceCollection services)
        {
            services.AddScoped<ITargetRepository, TargetRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<TargetValueDTO, TargetValue>().ReverseMap();
            });
        }
    }
}
