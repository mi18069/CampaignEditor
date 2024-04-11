
namespace Database.DTOs.MediaPlanRealizedDTO
{
    public class BaseMediaPlanRealizedDTO
    {
        public BaseMediaPlanRealizedDTO(string name, int stime, int etime, int chid, int dure, int durf, string date, int emsnum, int posinbr, int totalspotnum, int breaktype, int spotnum, int brandnum, double amrp1, double amrp2, double amrp3, double amrpsale, double cpp, double dpcoef, double seascoef, double seccoef, double progcoef, double price, int status)
        {
            this.name = name;
            this.stime = stime;
            this.etime = etime;
            this.chid = chid;
            this.dure = dure;
            this.durf = durf;
            this.date = date;
            this.emsnum = emsnum;
            this.posinbr = posinbr;
            this.totalspotnum = totalspotnum;
            this.breaktype = breaktype;
            this.spotnum = spotnum;
            this.brandnum = brandnum;
            this.amrp1 = amrp1;
            this.amrp2 = amrp2;
            this.amrp3 = amrp3;
            this.amrpsale = amrpsale;
            this.cpp = cpp;
            this.dpcoef = dpcoef;
            this.seascoef = seascoef;
            this.seccoef = seccoef;
            this.progcoef = progcoef;
            this.price = price;
            this.status = status;
        }

        public string name { get; set; }
        public int stime { get; set; }
        public int etime { get; set; }
        public int chid { get; set; }
        public int dure { get; set; }
        public int durf { get; set; }
        public string date { get; set; }
        public int emsnum { get; set; }
        public int posinbr { get; set; }
        public int totalspotnum { get; set; }
        public int breaktype { get; set; }
        public int spotnum { get; set; }
        public int brandnum { get; set; }
        public double amrp1 { get; set; }
        public double amrp2 { get; set; }
        public double amrp3 { get; set; }
        public double amrpsale { get; set; }
        public double cpp { get; set; }
        public double dpcoef { get; set; }
        public double seascoef { get; set; }
        public double seccoef { get; set; }
        public double progcoef { get; set; }
        public double price { get; set; }
        public int status { get; set; }

    }
}
