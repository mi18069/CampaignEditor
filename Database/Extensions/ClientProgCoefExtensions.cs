using Database.DTOs.ClientProgCoefDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ClientProgCoefExtensions
    {
        public static void AddClientProgCoefExtensions(this IServiceCollection services)
        {
            services.AddScoped<IClientProgCoefRepository, ClientProgCoefRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<ClientProgCoefDTO, ClientProgCoef>().ReverseMap();
            });

        }
    }
}
