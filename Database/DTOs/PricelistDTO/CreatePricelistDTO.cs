﻿
namespace Database.DTOs.PricelistDTO
{
    public class CreatePricelistDTO : BasePricelistDTO
    {
        public CreatePricelistDTO(int clid, string plname, int pltype, int sectbid, int seastbid, bool plactive, decimal price, decimal minprice, bool prgcoef, int pltarg, bool use2, int sectbid2, int sectb2st, int sectb2en, int valfrom, int valto, bool mgtype, decimal fixprice) 
            : base(clid, plname, pltype, sectbid, seastbid, plactive, price, minprice, prgcoef, pltarg, use2, sectbid2, sectb2st, sectb2en, valfrom, valto, mgtype, fixprice)
        {
        }

        public CreatePricelistDTO(PricelistDTO pricelist)
            : base(pricelist.clid, pricelist.plname, pricelist.pltype, pricelist.sectbid, pricelist.seastbid, pricelist.plactive, pricelist.price, pricelist.minprice, pricelist.prgcoef, pricelist.pltarg, pricelist.use2, pricelist.sectbid2, pricelist.sectb2st, pricelist.sectb2en, pricelist.valfrom, pricelist.valto, pricelist.mgtype, pricelist.fixprice)
        {
        }
    }
}
