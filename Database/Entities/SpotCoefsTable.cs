﻿using Database.DTOs.SpotDTO;
using System;

namespace Database.Entities
{
    public class SpotCoefsTable
    {
        private SpotDTO spot { get; set; }
        private DateRangeSeasCoef dateRange {get; set;}
        private double secCoef { get; set; }
        private double price { get; set; }

        public string Spot { get { return spot.spotname.Trim(); } }
        public string Date { get { return dateRange.ToString(); } }
        public double Seas { get { return Math.Round(dateRange.seascoef, 2); } }
        public double Sec { get { return Math.Round(secCoef, 2); } }
        public double Price { get { return Math.Round(price, 2); } }

        public SpotCoefsTable(SpotDTO spot, DateRangeSeasCoef dateRange, double secCoef, double price)
        {
            this.dateRange = dateRange;
            this.secCoef = secCoef;
            this.spot = spot;
            this.price = price;
        }
    }
}
