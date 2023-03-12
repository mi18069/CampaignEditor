using Database.DTOs.GoalsDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class GoalsExtensions
    {
        public static void AddGoalsExtensions(this IServiceCollection services)
        {
            services.AddScoped<IGoalsRepository, GoalsRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<GoalsDTO, Goals>().ReverseMap();
            });

        }
    }
}
