using System;

namespace Database.DTOs.PricesDTO
{
    public class BasePricesDTO
    {
        public BasePricesDTO(int plid, string dps, string dpe, float price, bool ispt)
        {
            this.plid = plid;
            this.dps = dps ?? throw new ArgumentNullException(nameof(dps));
            this.dpe = dpe ?? throw new ArgumentNullException(nameof(dpe));
            this.price = price;
            this.ispt = ispt;
        }

        public int plid { get; set; }
        public string dps { get; set; }
        public string dpe { get; set; }
        public float price { get; set; }
        public bool ispt { get; set; }
    }
}
