namespace Database.DTOs.DayPartDTO
{
    public class BaseDayPartDTO
    {
        public BaseDayPartDTO(int clid, string name, string days)
        {
            this.clid = clid;
            this.name = name;
            this.days = days;
        }

        public int clid { get; set; }
        public string name { get; set; }
        public string days { get; set; }
    }
}
