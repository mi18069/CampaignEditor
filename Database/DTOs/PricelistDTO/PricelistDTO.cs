
namespace Database.DTOs.PricelistDTO
{
    public class PricelistDTO : BaseIdentityPricelistDTO
    {
        public PricelistDTO(int plid, int clid, string plname, int pltype, int sectbid, int seastbid, bool plactive, float price, float minprice, bool prgcoef, int pltarg, string a2chn, bool use2, int sectbid2, int sectb2st, int sectb2en, int valfrom, int valto, bool mgtype) : base(plid, clid, plname, pltype, sectbid, seastbid, plactive, price, minprice, prgcoef, pltarg, a2chn, use2, sectbid2, sectb2st, sectb2en, valfrom, valto, mgtype)
        {
        }
    }
}
