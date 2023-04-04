using Database.DTOs.MediaPlanDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static  class MediaPlanExtensions
    {
        public static void AddMediaPlanExtensions(this IServiceCollection services)
        {
            services.AddScoped<IMediaPlanRepository, MediaPlanRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<MediaPlanDTO, MediaPlan>().ReverseMap();
            });

        }
    }
}
