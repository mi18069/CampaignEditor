using System;

namespace Database.Entities
{
    public class MediaPlanDBEntity
    {
        public int xmpid { get; set; }
        public int schid { get; set; }
        public int cmpid { get; set; }
        public int chid { get; set; }
        public string naziv { get; set; }
        public int verzija { get; set; }
        public string pozicija { get; set; }
        public string vremeod { get; set; }
        public string? vremedo { get; set; }
        public string? vremerbl { get; set; }
        public string dani { get; set; }
        public string tipologija { get; set; }
        public bool specijal { get; set; }
        public DateTime datumod { get; set; }
        public DateTime? datumdo { get; set; }
        public decimal progkoef { get; set; }
        public DateTime datumkreiranja { get; set; }
        public DateTime? datumizmene { get; set; }
        public decimal amr1 { get; set; }
        public int amr1trim { get; set; }
        public decimal amr2 { get; set; }
        public int amr2trim { get; set; }
        public decimal amr3 { get; set; }
        public int amr3trim { get; set; }
        public decimal amrsale { get; set; }
        public int amrsaletrim { get; set; }
        public decimal amrp1 { get; set; }
        public decimal amrp2 { get; set; }
        public decimal amrp3 { get; set; }
        public decimal amrpsale { get; set; }
        public decimal dpkoef { get; set; }
        public decimal seaskoef { get; set; }
        public decimal seckoef { get; set; }
        public decimal price { get; set; }
        public bool active { get; set; }
        public decimal pricepersec { get; set; }
        public decimal koefa { get; set; }
        public decimal koefb { get; set; }
    }
}
