
namespace Database.DTOs.SpotDTO
{
    public class UpdateSpotDTO : BaseIdentitySpotDTO
    {
        public UpdateSpotDTO(int cmpid, string spotcode, string spotname, int spotlength, bool ignore) 
            : base(cmpid, spotcode, spotname, spotlength, ignore)
        {
        }
    }
}
