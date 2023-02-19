
namespace Database.DTOs.PricelistChannels
{
    public class UpdatePricelistChannelsDTO : BaseIdentityPricelistChannelsDTO
    {
        public UpdatePricelistChannelsDTO(int plid, int chid) : base(plid, chid)
        {
        }
    }
}
