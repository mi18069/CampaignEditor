using Database.DTOs.TargetCmpDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class TargetCmpExtensions
    {
        public static void AddTargetCmpExtensions(this IServiceCollection services)
        {
            services.AddScoped<ITargetCmpRepository, TargetCmpRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<TargetCmpDTO, TargetCmp>().ReverseMap();
            });

        }
    }
}
