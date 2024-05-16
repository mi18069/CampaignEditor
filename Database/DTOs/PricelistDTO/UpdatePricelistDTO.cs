
namespace Database.DTOs.PricelistDTO
{
    public class UpdatePricelistDTO : BaseIdentityPricelistDTO
    {
        public UpdatePricelistDTO(int plid, int clid, string plname, int pltype, int sectbid, int seastbid, bool plactive, decimal price, decimal minprice, bool prgcoef, int pltarg, bool use2, int sectbid2, int sectb2st, int sectb2en, int valfrom, int valto, bool mgtype) 
            : base(plid, clid, plname, pltype, sectbid, seastbid, plactive, price, minprice, prgcoef, pltarg, use2, sectbid2, sectb2st, sectb2en, valfrom, valto, mgtype)
        {
        }
    }
}
