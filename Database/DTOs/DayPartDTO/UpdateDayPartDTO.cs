
namespace Database.DTOs.DayPartDTO
{
    public class UpdateDayPartDTO : BaseIdentityDayPartDTO
    {
        public UpdateDayPartDTO(int dpid, int clid, string name, string days) 
            : base(dpid, clid, name, days)
        {
        }
    }
}
