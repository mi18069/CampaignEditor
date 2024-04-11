using Database.DTOs.MediaPlanRealizedDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class MediaPlanRealizedExtensions
    {
        public static void AddMediaPlanRealizedExtensions(this IServiceCollection services)
        {
            services.AddScoped<IMediaPlanRealizedRepository, MediaPlanRealizedRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<MediaPlanRealizedDTO, MediaPlanRealized>().ReverseMap();
            });
        }
    }
}
