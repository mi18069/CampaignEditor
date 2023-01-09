using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Entities;
using CampaignEditor.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignEditor.Extensions
{
    public static class UserExtensions
    {
        public static void AddUserExtensions(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<UserDTO, User>().ReverseMap();
            });
            
        }
    }
}
