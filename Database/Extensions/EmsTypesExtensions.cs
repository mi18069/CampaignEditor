using Database.DTOs.EmsTypesDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class EmsTypesExtensions
    {
        public static void AddEmsTypesExtensions(this IServiceCollection services)
        {
            services.AddScoped<IEmsTypesRepository, EmsTypesRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<EmsTypesDTO, EmsTypes>().ReverseMap();
            });

        }
    }
}
