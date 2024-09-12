namespace Database.DTOs.RealizedSpotDTO
{
    public class CreateRealizedSpotDTO : BaseRealizedSpotDTO
    {
        public CreateRealizedSpotDTO(int brandnum, int row, string spotname, int spotlength, bool active, int variant, string firstdate) : base(brandnum, row, spotname, spotlength, active, variant, firstdate)
        {
        }
    }
}
