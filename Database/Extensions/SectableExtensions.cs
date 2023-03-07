using Database.DTOs.SectableDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static  class SectableExtensions
    {
        public static void AddSectableExtensions(this IServiceCollection services)
        {
            services.AddScoped<ISectableRepository, SectableRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<SectableDTO, Sectable>().ReverseMap();
            });

        }
    }
}
