
namespace Database.DTOs.SeasonalityDTO
{
    public class UpdateSeasonalityDTO : BaseIdentitySeasonalityDTO
    {
        public UpdateSeasonalityDTO(int seasid, string seasname, bool seasactive, int ownedby) 
            : base(seasid, seasname, seasactive, ownedby)
        {
        }
    }
}
