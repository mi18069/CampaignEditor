
namespace Database.DTOs.SeasonalityDTO
{
    public class CreateSeasonalityDTO : BaseSeasonalityDTO
    {
        public CreateSeasonalityDTO(string seasname, bool seasactive, int ownedby) 
            : base(seasname, seasactive, ownedby)
        {
        }
    }
}
