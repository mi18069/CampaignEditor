
namespace Database.DTOs.SeasonalityChannelsDTO
{
    public class SeasonalityChannelsDTO : BaseIdentitySeasonalityChannelsDTO
    {
        public SeasonalityChannelsDTO(int seasid, int chid) 
            : base(seasid, chid)
        {
        }
    }
}
