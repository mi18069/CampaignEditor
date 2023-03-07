
namespace Database.DTOs.SeasonalityDTO
{
    public class SeasonalityDTO : BaseIdentitySeasonalityDTO
    {
        public SeasonalityDTO(int seasid, string seasname, bool seasactive, int ownedby) 
            : base(seasid, seasname, seasactive, ownedby)
        {
        }
    }
}
