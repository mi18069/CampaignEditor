
namespace Database.DTOs.SeasonalityChannelsDTO
{
    public class CreateSeasonalityChannelsDTO : BaseIdentitySeasonalityChannelsDTO
    {
        public CreateSeasonalityChannelsDTO(int seasid, int chid) 
            : base(seasid, chid)
        {
        }
    }
}
