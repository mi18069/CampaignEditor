using Database.DTOs.MediaPlanVersionDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class MediaPlanVersionExtension
    {
        public static void AddMediaPlanVersionExtensions(this IServiceCollection services)
        {
            services.AddScoped<IMediaPlanVersionRepository, MediaPlanVersionRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<MediaPlanVersionDTO, MediaPlanVersion>().ReverseMap();
            });

        }
    }
}
