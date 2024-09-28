namespace Database.DTOs.SpotPairDTO
{
    public class UpdateSpotPairDTO : BaseSpotPairDTO
    {
        public UpdateSpotPairDTO(int cmpid, string spotcode, int spotnum) : base(cmpid, spotcode, spotnum)
        {
        }

        public UpdateSpotPairDTO(SpotPairDTO spDTO) : base(spDTO.cmpid, spDTO.spotcode, spDTO.spotnum)
        {
        }
    }
}
