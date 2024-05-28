
namespace Database.DTOs.SpotDTO
{
    public class SpotDTO : BaseIdentitySpotDTO
    {
        public SpotDTO(int cmpid, string spotcode, string spotname, int spotlength, bool ignore) 
            : base(cmpid, spotcode, spotname, spotlength, ignore)
        {
        }

        public string SpotName { get { return spotname.Trim(); } }
    }
}
