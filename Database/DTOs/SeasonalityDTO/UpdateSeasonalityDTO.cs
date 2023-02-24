
namespace Database.DTOs.SeasonalityDTO
{
    public class UpdateSeasonalityDTO : BaseIdentitySeasonalityDTO
    {
        public UpdateSeasonalityDTO(int seasid, string seasname, bool seasactive, string channel, int ownedby) 
            : base(seasid, seasname, seasactive, channel, ownedby)
        {
        }
    }
}
