using System;

namespace Database.DTOs.MediaPlanDTO
{
    public class BaseMediaPlanDTO
    {
        public BaseMediaPlanDTO(int schid, int cmpid, int chid, string name, int version, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, decimal progcoef, DateOnly created, DateOnly? modified, decimal amr1, int amr1trim, decimal amr2, int amr2trim, decimal amr3, int amr3trim, decimal amrsale, int amrsaletrim, decimal amrp1, decimal amrp2, decimal amrp3, decimal amrpsale, decimal dpcoef, decimal seascoef, decimal seccoef, decimal coefA, decimal coefB, decimal price, bool active, decimal pps)
        {
            this.schid = schid;
            this.cmpid = cmpid;
            this.chid = chid;
            this.name = name;
            this.version = version;
            this.position = position;
            this.stime = stime;
            this.etime = etime;
            this.blocktime = blocktime;
            this.days = days;
            this.type = type;
            this.special = special;
            this.sdate = sdate;
            this.edate = edate;
            this.progcoef = progcoef;
            this.created = created;
            this.modified = modified;
            this.amr1 = amr1;
            this.amr1trim = amr1trim;
            this.amr2 = amr2;
            this.amr2trim = amr2trim;
            this.amr3 = amr3;
            this.amr3trim = amr3trim;
            this.amrsale = amrsale;
            this.amrsaletrim = amrsaletrim;
            this.amrp1 = amrp1;
            this.amrp2 = amrp2;
            this.amrp3 = amrp3;
            this.amrpsale = amrpsale;
            this.dpcoef = dpcoef;
            this.seascoef = seascoef;
            this.seccoef = seccoef;
            this.coefA = coefA;
            this.coefB = coefB;
            this.price = price;
            this.active = active;
            this.pps = pps;
        }

        public int schid { get; set; }
        public int cmpid { get; set; }
        public int chid { get; set; }
        public string name { get; set; }
        public int version { get; set; }
        public string position { get; set; }
        public string stime { get; set; }
        public string? etime { get; set; }
        public string? blocktime { get; set; }
        public string days { get; set; }
        public string type { get; set; }
        public bool special { get; set; }
        public DateOnly sdate { get; set; }
        public DateOnly? edate { get; set; }
        public decimal progcoef { get; set; }
        public DateOnly created { get; set; }
        public DateOnly? modified { get; set; }
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
        public decimal dpcoef { get; set; }
        public decimal seascoef { get; set; }
        public decimal seccoef { get; set; }
        public decimal coefA { get; set; }
        public decimal coefB { get; set; }
        public decimal price { get; set; }
        public bool active { get; set; }

        public decimal pps { get; set; }

    }
}
