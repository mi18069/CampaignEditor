using Database.DTOs.MediaPlanTermDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class MediaPlanTermExtensions
    {
        public static void AddMediaPlanTermExtensions(this IServiceCollection services)
        {
            services.AddScoped<IMediaPlanTermRepository, MediaPlanTermRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<MediaPlanTermDTO, MediaPlanTerm>().ReverseMap();
            });

        }
    }
}
