using Database.DTOs.ReachDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ReachExtensions
    {
        public static void AddReachExtensions(this IServiceCollection services)
        {
            services.AddScoped<IReachRepository, ReachRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<ReachDTO, Reach>().ReverseMap();
            });
        }
    }
}
