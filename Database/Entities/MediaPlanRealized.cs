namespace Database.Entities
{
    public class MediaPlanRealized
    {
        public int id { get; set; }
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
