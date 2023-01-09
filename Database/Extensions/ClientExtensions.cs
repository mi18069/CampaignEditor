using Database.DTOs.ClientDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ClientExtensions
    {
        public static void AddClientExtensions(this IServiceCollection services)
        {
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<ClientDTO, Client>().ReverseMap();
            });
        }
    }
}
