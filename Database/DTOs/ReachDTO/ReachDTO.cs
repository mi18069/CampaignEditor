using System;

namespace Database.DTOs.ReachDTO
{
    public class ReachDTO : BaseIdentityReachDTO
    {
        public ReachDTO(int id, int cmpid, string date, int xmpid, int channel, int time, decimal? universe, string demostr1, string demostr2, int num1, decimal universe1, int num2, decimal universe2, decimal ogrp1, decimal grp1, decimal rchn1, decimal rch1, decimal rch12, decimal rch13, decimal rch14, decimal rch15, decimal rch16, decimal rch17, decimal rch18, decimal? rch19, decimal ogrp2, decimal grp2, decimal rchn2, decimal rch2, decimal rch22, decimal rch23, decimal rch24, decimal rch25, decimal rch26, decimal rch27, decimal rch28, decimal? rch29, int live, int adi, int mdi, int adi2, int mdi2, int? redbrems) : base(id, cmpid, date, xmpid, channel, time, universe, demostr1, demostr2, num1, universe1, num2, universe2, ogrp1, grp1, rchn1, rch1, rch12, rch13, rch14, rch15, rch16, rch17, rch18, rch19, ogrp2, grp2, rchn2, rch2, rch22, rch23, rch24, rch25, rch26, rch27, rch28, rch29, live, adi, mdi, adi2, mdi2, redbrems)
        {
        }

        public decimal Rch1 { get { return Math.Min(rch1, 100); } }
        public decimal Rch12 { get { return Math.Min(rch12, 100); } }
        public decimal Rch13 { get { return Math.Min(rch13, 100); } }
        public decimal Rch14 { get { return Math.Min(rch14, 100); } }
        public decimal Rch15 { get { return Math.Min(rch15, 100); } }
        public decimal Rch16 { get { return Math.Min(rch16, 100); } }
        public decimal Rch17 { get { return Math.Min(rch17, 100); } }
        public decimal Rch18 { get { return Math.Min(rch18, 100); } }
        public decimal Rch19 { get { return Math.Min(rch19 ?? 0, 100); } }

        public decimal Rch2 { get { return Math.Min(rch2, 100); } }
        public decimal Rch22 { get { return Math.Min(rch22, 100); } }
        public decimal Rch23 { get { return Math.Min(rch23, 100); } }
        public decimal Rch24 { get { return Math.Min(rch24, 100); } }
        public decimal Rch25 { get { return Math.Min(rch25, 100); } }
        public decimal Rch26 { get { return Math.Min(rch26, 100); } }
        public decimal Rch27 { get { return Math.Min(rch27, 100); } }
        public decimal Rch28 { get { return Math.Min(rch28, 100); } }
        public decimal Rch29 { get { return Math.Min(rch29 ?? 0, 100); } }
    }
}
