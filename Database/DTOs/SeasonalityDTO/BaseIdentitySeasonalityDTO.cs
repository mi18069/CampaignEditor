
namespace Database.DTOs.SeasonalityDTO
{
    public class BaseIdentitySeasonalityDTO : BaseSeasonalityDTO
    {
        public BaseIdentitySeasonalityDTO(int seasid, string seasname, bool seasactive, int ownedby) 
            : base(seasname, seasactive, ownedby)
        {
            this.seasid = seasid;
        }

        public int seasid { get; set; }
       
    }
}
