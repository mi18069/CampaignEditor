using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor
{
    public class CampaignInfo
    {      
        public CampaignInfo()
        {
        }
        public CampaignDTO _campaign { get; set; }

    }
}
