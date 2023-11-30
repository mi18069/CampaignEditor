using Database.DTOs.CampaignDTO;
using CampaignEditor.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignEditor.Extensions
{
    public static class CampaignExtensions 
    {
        public static void AddCampaignExtensions(this IServiceCollection services)
        {
            services.AddScoped<ICampaignRepository, CampaignRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<CampaignDTO, Campaign>().ReverseMap();
            });

        }
    }
}
