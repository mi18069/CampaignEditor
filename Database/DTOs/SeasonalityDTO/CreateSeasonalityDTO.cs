
namespace Database.DTOs.SeasonalityDTO
{
    public class CreateSeasonalityDTO : BaseSeasonalityDTO
    {
        public CreateSeasonalityDTO(string seasname, bool seasactive, string channel, int ownedby) 
            : base(seasname, seasactive, channel, ownedby)
        {
        }
    }
}
