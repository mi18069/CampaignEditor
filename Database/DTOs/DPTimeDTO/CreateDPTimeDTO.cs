
namespace Database.DTOs.DPTimeDTO
{
    public class CreateDPTimeDTO : BaseDPTimeDTO
    {
        public CreateDPTimeDTO(int dpid, string stime, string etime) 
            : base(dpid, stime, etime)
        {
        }
    }
}
