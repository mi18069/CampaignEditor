using Database.DTOs.UserClients;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class UserClientsExtensions
    {
        public static void AddUserClientsExtensions(this IServiceCollection services)
        {
            services.AddScoped<IUserClientsRepository, UserClientsRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<UserClientsDTO, UserClients>().ReverseMap();
            });
        }

    }
}
