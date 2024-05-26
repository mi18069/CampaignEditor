
namespace Database.DTOs.PricelistChannels
{
    public class CreatePricelistChannelsDTO : BaseIdentityPricelistChannelsDTO
    {
        public CreatePricelistChannelsDTO(int plid, int chid, decimal chcoef) 
            : base(plid, chid, chcoef)
        {
        }
    }
}
