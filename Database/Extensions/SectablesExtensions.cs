using Database.DTOs.SectablesDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class SectablesExtensions
    {
        public static void AddSectablesExtensions(this IServiceCollection services)
        {
            services.AddScoped<ISectablesRepository, SectablesRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<SectablesDTO, Sectables>().ReverseMap();
            });

        }
    }
}
