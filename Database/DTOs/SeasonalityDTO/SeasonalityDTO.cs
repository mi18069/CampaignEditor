
namespace Database.DTOs.SeasonalityDTO
{
    public class SeasonalityDTO : BaseIdentitySeasonalityDTO
    {
        public SeasonalityDTO(int seasid, string seasname, bool seasactive, string channel, int ownedby) : base(seasid, seasname, seasactive, channel, ownedby)
        {
        }
    }
}
