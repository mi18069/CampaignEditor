
namespace Database.DTOs.DPTimeDTO
{
    public class DPTimeDTO : BaseIdentityDPTimeDTO
    {
        public DPTimeDTO(int dptimeid, int dpid, string stime, string etime) 
            : base(dptimeid, dpid, stime, etime)
        {
        }
    }
}
