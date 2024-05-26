
namespace Database.DTOs.PricelistChannels
{
    public class UpdatePricelistChannelsDTO : BaseIdentityPricelistChannelsDTO
    {
        public UpdatePricelistChannelsDTO(int plid, int chid, decimal chcoef) : base(plid, chid, chcoef)
        {
        }
    }
}
