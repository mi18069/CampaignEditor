using Database.DTOs.MediaPlanRef;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class MediaPlanRefExtensions
    {
        public static void AddMediaPlanRefExtensions(this IServiceCollection services)
        {
            services.AddScoped<IMediaPlanRefRepository, MediaPlanRefRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<MediaPlanRefDTO, MediaPlanRef>().ReverseMap();
            });

        }
    }
}
