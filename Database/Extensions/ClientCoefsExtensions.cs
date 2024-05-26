using Database.DTOs.ClientCoefsDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ClientCoefsExtensions
    {
        public static void AddClientCoefsExtensions(this IServiceCollection services)
        {
            services.AddScoped<IClientCoefsRepository, ClientCoefsRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<ClientCoefsDTO, ClientCoefs>().ReverseMap();
            });

        }
    }
}
