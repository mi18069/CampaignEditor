namespace Database.DTOs.SpotPairDTO
{
    public class CreateSpotPairDTO : BaseSpotPairDTO
    {
        public CreateSpotPairDTO(int cmpid, string spotcode, int spotnum) : base(cmpid, spotcode, spotnum)
        {
        }
    }
}
