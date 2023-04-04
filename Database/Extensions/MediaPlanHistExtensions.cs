using Database.DTOs.MediaPlanHistDTO;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class MediaPlanHistExtensions
    {
        public static void AddMediaPlanHistExtensions(this IServiceCollection services)
        {
            services.AddScoped<IMediaPlanHistRepository, MediaPlanHistRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<MediaPlanHistDTO, MediaHistPlan>().ReverseMap();
            });

        }
    }
}
