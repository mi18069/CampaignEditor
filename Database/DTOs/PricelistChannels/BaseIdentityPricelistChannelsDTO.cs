
namespace Database.DTOs.PricelistChannels
{
    public class BaseIdentityPricelistChannelsDTO
    {
        public BaseIdentityPricelistChannelsDTO(int plid, int chid)
        {
            this.plid = plid;
            this.chid = chid;
        }

        public int plid { get; set; }
        public int chid { get; set; }
    }
}
