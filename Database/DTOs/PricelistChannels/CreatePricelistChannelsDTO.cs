
namespace Database.DTOs.PricelistChannels
{
    public class CreatePricelistChannelsDTO : BaseIdentityPricelistChannelsDTO
    {
        public CreatePricelistChannelsDTO(int plid, int chid) 
            : base(plid, chid)
        {
        }
    }
}
