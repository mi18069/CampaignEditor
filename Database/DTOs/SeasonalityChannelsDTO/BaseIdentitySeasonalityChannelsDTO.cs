
namespace Database.DTOs.SeasonalityChannelsDTO
{
    public class BaseIdentitySeasonalityChannelsDTO
    {
        public BaseIdentitySeasonalityChannelsDTO(int seasid, int chid)
        {
            this.seasid = seasid;
            this.chid = chid;
        }

        public int seasid { get; set; }
        public int chid { get; set; }
    }
}
