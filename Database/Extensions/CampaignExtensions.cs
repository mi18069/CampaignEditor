using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.Entities;
using CampaignEditor.Repositories;
using Database.Data;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
