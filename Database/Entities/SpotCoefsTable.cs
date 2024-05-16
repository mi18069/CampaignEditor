using Database.DTOs.SpotDTO;
using System;

namespace Database.Entities
{
    public class SpotCoefsTable
    {
        private SpotDTO spot { get; set; }
        private DateRangeSeasCoef dateRange {get; set;}
        private decimal secCoef { get; set; }
        private decimal price { get; set; }

        public string Spotcode { get { return spot.spotcode.Trim(); } }
        public string Seconds { get { return spot.spotlength.ToString();  } }
        public string Spot { get { return spot.spotname.Trim(); } }
        public string Date { get { return dateRange.ToString(); } }
        public decimal Seas { get { return Math.Round(dateRange.seascoef, 2); } }
        public decimal Sec { get { return Math.Round(secCoef, 2); } }
        public decimal Price { get { return Math.Round(price, 2); } }

        public SpotCoefsTable(SpotDTO spot, DateRangeSeasCoef dateRange, decimal secCoef, decimal price)
        {
            this.dateRange = dateRange;
            this.secCoef = secCoef;
            this.spot = spot;
            this.price = price;
        }
    }
}
