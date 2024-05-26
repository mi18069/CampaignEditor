
namespace Database.DTOs.PricelistChannels
{
    public class PricelistChannelsDTO : BaseIdentityPricelistChannelsDTO
    {
        public PricelistChannelsDTO(int plid, int chid, decimal chcoef) 
            : base(plid, chid, chcoef)
        {
        }
    }
}
