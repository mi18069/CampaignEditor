namespace Database.DTOs.DayPartDTO
{
    public class DayPartDTO : BaseIdentityDayPartDTO
    {
        public DayPartDTO(int dpid, int clid, string name, string days) 
            : base(dpid, clid, name, days)
        {
        }
    }
}
