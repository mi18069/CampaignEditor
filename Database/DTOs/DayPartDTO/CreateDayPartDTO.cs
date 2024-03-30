
namespace Database.DTOs.DayPartDTO
{
    public class CreateDayPartDTO : BaseDayPartDTO
    {
        public CreateDayPartDTO(int clid, string name, string days) 
            : base(clid, name, days)
        {
        }
    }
}
