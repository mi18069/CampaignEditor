
namespace Database.DTOs.DayPartDTO
{
    public class BaseIdentityDayPartDTO : BaseDayPartDTO
    {
        public BaseIdentityDayPartDTO(int dpid, int clid, string name, string days) 
            : base(clid, name, days)
        {
            this.dpid = dpid;
        }

        public int dpid { get; set; }
    }
}
