
namespace Database.DTOs.PricelistChannels
{
    public class BaseIdentityPricelistChannelsDTO
    {
        public BaseIdentityPricelistChannelsDTO(int plid, int chid, decimal chcoef)
        {
            this.plid = plid;
            this.chid = chid;
            this.chcoef = chcoef;
        }

        public int plid { get; set; }
        public int chid { get; set; }
        public decimal chcoef { get; set; }
    }
}
