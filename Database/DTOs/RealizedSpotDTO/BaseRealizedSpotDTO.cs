namespace Database.DTOs.RealizedSpotDTO
{
    public class BaseRealizedSpotDTO
    {
        public BaseRealizedSpotDTO(int brandnum, int? row, string spotname, int spotlength, bool? active, int? variant, string firstdate)
        {
            this.brandnum = brandnum;
            this.row = row;
            this.spotname = spotname;   
            this.spotlength = spotlength;
            this.active = active;
            this.variant = variant;
            this.firstdate = firstdate;
        }

        public int brandnum { get; set; }
        public int? row { get; set; }
        public string spotname { get; set; }
        public int spotlength { get; set; }
        public bool? active { get; set; }
        public int? variant { get; set; }
        public string firstdate { get; set; }
    }
}
