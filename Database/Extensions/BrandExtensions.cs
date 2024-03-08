using Database.DTOs.BrandDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class BrandExtensions
    {
        public static void AddBrandExtensions(this IServiceCollection services)
        {
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddAutoMapper(configuration =>
            {
                configuration.CreateMap<BrandDTO, Brand>().ReverseMap();
            });
        }
        
    }
}
