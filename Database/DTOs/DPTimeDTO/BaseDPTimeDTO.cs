namespace Database.DTOs.DPTimeDTO
{
    public class BaseDPTimeDTO
    {
        public BaseDPTimeDTO(int dpid, string stime, string etime)
        {
            this.dpid = dpid;
            this.stime = stime;
            this.etime = etime;
        }

        public int dpid { get; set; }
        public string stime { get; set; }
        public string etime { get; set; }
    }
}
