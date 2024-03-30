namespace Database.DTOs.DPTimeDTO
{
    public class BaseIdentityDPTimeDTO : BaseDPTimeDTO
    {
        public BaseIdentityDPTimeDTO(int dptimeid, int dpid, string stime, string etime) 
            : base(dpid, stime, etime)
        {
            this.dptimeid = dptimeid;
        }

        public int dptimeid { get; set; }
    }
}
