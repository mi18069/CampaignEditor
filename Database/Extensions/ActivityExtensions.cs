using Database.DTOs.ActivityDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ActivityExtensions
    {
        public static void AddActivityExtensions(this IServiceCollection services)
        {
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<ActivityDTO, Activity>().ReverseMap();
            });

        }
    }
}
