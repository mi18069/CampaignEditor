
namespace Database.DTOs.SeasonalityChannelsDTO
{
    public class UpdateSeasonalityChannelsDTO : BaseIdentitySeasonalityChannelsDTO
    {
        public UpdateSeasonalityChannelsDTO(int seasid, int chid) 
            : base(seasid, chid)
        {
        }
    }
}
