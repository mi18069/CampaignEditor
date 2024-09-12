namespace Database.DTOs.RealizedSpotDTO
{
    public class RealizedSpotDTO : BaseIdentityRealizedSpotDTO
    {
        public RealizedSpotDTO(int spotnum, int brandnum, int row, string spotname, int spotlength, bool active, int variant, string firstdate) : base(spotnum, brandnum, row, spotname, spotlength, active, variant, firstdate)
        {
        }
    }
}
