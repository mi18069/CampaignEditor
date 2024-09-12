namespace Database.DTOs.RealizedSpotDTO
{
    public class BaseIdentityRealizedSpotDTO : BaseRealizedSpotDTO
    {
        public BaseIdentityRealizedSpotDTO(int spotnum, int brandnum, int row, string spotname, int spotlength, bool active, int variant, string firstdate) : base(brandnum, row, spotname, spotlength, active, variant, firstdate)
        {
            this.spotnum = spotnum;
        }

        public int spotnum { get; set; }
    }
}
