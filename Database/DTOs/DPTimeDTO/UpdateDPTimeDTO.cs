
namespace Database.DTOs.DPTimeDTO
{
    public class UpdateDPTimeDTO : BaseIdentityDPTimeDTO
    {
        public UpdateDPTimeDTO(int dptimeid, int dpid, string stime, string etime) 
            : base(dptimeid, dpid, stime, etime)
        {
        }
    }
}
