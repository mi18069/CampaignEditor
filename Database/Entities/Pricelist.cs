
namespace Database.Entities
{
    public class Pricelist
    {
        public int plid { get; set; }
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
