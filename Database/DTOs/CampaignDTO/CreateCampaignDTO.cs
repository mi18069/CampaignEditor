using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.DTOs.CampaignDTO
{
    public class CreateCampaignDTO : BaseCampaignDTO
    {
        public CreateCampaignDTO(int cmprev, int cmpown, string cmpname, int clid, string cmpsdate, string cmpedate,
            string cmpstime, string cmpetime, int cmpstatus, string sostring, int activity, int cmpaddedon,
            int cmpaddedat, bool active, bool forcec)
            : base(cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate,
            cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon,
            cmpaddedat, active, forcec)
        {
        }

        public CreateCampaignDTO(CampaignDTO campaignDTO)
            : base(campaignDTO.cmprev, campaignDTO.cmpown, campaignDTO.cmpname, campaignDTO.clid, campaignDTO.cmpsdate, campaignDTO.cmpedate,
            campaignDTO.cmpstime, campaignDTO.cmpetime, campaignDTO.cmpstatus, campaignDTO.sostring, campaignDTO.activity, campaignDTO.cmpaddedon,
            campaignDTO.cmpaddedat, campaignDTO.active, campaignDTO.forcec)
        {
        }
    }
}
