
namespace Database.DTOs.CampaignDTO
{
    public class BaseIdentityCampaignDTO : BaseCampaignDTO
    {
        public BaseIdentityCampaignDTO(int cmpid, int cmprev, int cmpown, string cmpname, int clid, string cmpsdate, string cmpedate,
            string cmpstime, string cmpetime, int cmpstatus, string sostring, int activity, int cmpaddedon,
            int cmpaddedat, bool active, bool forcec, bool tv)
            :base(cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate,
            cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon,
            cmpaddedat, active, forcec, tv)
        {
            this.cmpid = cmpid;
        }

        public int cmpid { get; set; }
    }
}
