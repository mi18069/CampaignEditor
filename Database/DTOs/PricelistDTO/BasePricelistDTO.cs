using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DTOs.PricelistDTO
{
    public class BasePricelistDTO
    {
        public BasePricelistDTO(int clid, string plname, int pltype, 
            int sectbid, int seastbid, bool plactive, float price, 
            float minprice, bool prgcoef, int pltarg, 
            bool use2, int sectbid2, int sectb2st, int sectb2en, 
            int valfrom, int valto, bool mgtype)
        {
            this.clid = clid;
            this.plname = plname ?? throw new ArgumentNullException(nameof(plname));
            this.pltype = pltype;
            this.sectbid = sectbid;
            this.seastbid = seastbid;
            this.plactive = plactive;
            this.price = price;
            this.minprice = minprice;
            this.prgcoef = prgcoef;
            this.pltarg = pltarg;
            this.use2 = use2;
            this.sectbid2 = sectbid2;
            this.sectb2st = sectb2st;
            this.sectb2en = sectb2en;
            this.valfrom = valfrom;
            this.valto = valto;
            this.mgtype = mgtype;
        }

        public int clid { get; set; }
        public string plname { get; set; }
        public int pltype { get; set; }
        public int sectbid { get; set; }
        public int seastbid { get; set; }
        public bool plactive { get; set; }
        public float price { get; set; }
        public float minprice { get; set; }
        public bool prgcoef { get; set; }
        public int pltarg { get; set; }
        public string a2chn { get; set; }
        public bool use2 { get; set; }
        public int sectbid2 { get; set; }
        public int sectb2st { get; set; }
        public int sectb2en { get; set; }
        public int valfrom { get; set; }
        public int valto { get; set; }
        public bool mgtype { get; set; }
    }
}
