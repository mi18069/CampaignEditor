
namespace Database.DTOs.MediaPlanRealizedDTO
{
    public class BaseMediaPlanRealizedDTO
    {
        public BaseMediaPlanRealizedDTO(int cmpid, string name, int stime, int etime, string stimestr, string etimestr, int chid, int dure, int durf, string date, int emsnum, int posinbr, int totalspotnum, int breaktype, int spotnum, int brandnum, decimal amrp1, decimal amrp2, decimal amrp3, decimal amrpsale, decimal cpp, decimal dpcoef, decimal seascoef, decimal seccoef, decimal progcoef, decimal price, int status, decimal chcoef, decimal coefA, decimal coefB)
        {
            this.cmpid = cmpid;
            this.name = name;
            this.stime = stime;
            this.etime = etime;
            this.stimestr = stimestr;
            this.etimestr = etimestr;
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
            this.chcoef = chcoef;
            this.coefA = coefA;
            this.coefB = coefB;
        }

        public int cmpid { get; set; }
        public string name { get; set; }
        public int stime { get; set; }
        public int etime { get; set; }
        public string stimestr { get; set; }
        public string etimestr { get; set; }
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
        public decimal amrp1 { get; set; }
        public decimal amrp2 { get; set; }
        public decimal amrp3 { get; set; }
        public decimal amrpsale { get; set; }
        public decimal cpp { get; set; }
        public decimal dpcoef { get; set; }
        public decimal seascoef { get; set; }
        public decimal seccoef { get; set; }
        public decimal progcoef { get; set; }
        public decimal price { get; set; }
        public int status { get; set; }
        public decimal chcoef { get; set; }
        public decimal coefA { get; set; }
        public decimal coefB { get; set; }

    }
}
