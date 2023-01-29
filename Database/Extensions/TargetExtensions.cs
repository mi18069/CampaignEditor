using Database.DTOs.TargetDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class TargetExtensions
    {
        public static void AddTargetExtensions(this IServiceCollection services)
        {
            services.AddScoped<ITargetRepository, TargetRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<TargetDTO, Target>().ReverseMap();
            });
        }
    }
}
