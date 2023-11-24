
namespace Database.DTOs.SpotDTO
{
    public class CreateSpotDTO : BaseIdentitySpotDTO
    {
        public CreateSpotDTO(int cmpid, string spotcode, string spotname, int spotlength, bool ignore) 
            : base(cmpid, spotcode, spotname, spotlength, ignore)
        {
        }

        public CreateSpotDTO(SpotDTO spot)
            : base(spot.cmpid, spot.spotcode, spot.spotname, spot.spotlength, spot.ignore)
        {
        }
    }
}
