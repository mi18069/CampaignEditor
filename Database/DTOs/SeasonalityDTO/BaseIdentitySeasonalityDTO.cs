
namespace Database.DTOs.SeasonalityDTO
{
    public class BaseIdentitySeasonalityDTO : BaseSeasonalityDTO
    {
        public BaseIdentitySeasonalityDTO(int seasid, string seasname, bool seasactive, string channel, int ownedby) 
            : base(seasname, seasactive, channel, ownedby)
        {
            this.seasid = seasid;
        }

        public int seasid { get; set; }
       
    }
}
