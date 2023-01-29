using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.DTOs.CampaignDTO
{
    public class UpdateCampaignDTO : BaseIdentityCampaignDTO
    {
        public UpdateCampaignDTO(int cmpid, int cmprev, int cmpown, string cmpname, int clid, string cmpsdate, string cmpedate,
            string cmpstime, string cmpetime, int cmpstatus, string sostring, int activity, int cmpaddedon,
            int cmpaddedat, bool active, bool forcec)
            : base(cmpid, cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate,
            cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon,
            cmpaddedat, active, forcec)
        {
        }
    }
}
