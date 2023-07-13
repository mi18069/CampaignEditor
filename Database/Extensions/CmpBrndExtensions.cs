using Database.DTOs.CmpBrndDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class CmpBrndExtensions
    {
        public static void AddCmpBrndExtensions(this IServiceCollection services)
        {
            services.AddScoped<ICmpBrndRepository, CmpBrndRepository>();
            services.AddAutoMapper(configuration =>
            {
                configuration.CreateMap<CmpBrndDTO, CmpBrnd>().ReverseMap();
            });
        }
    }
}
