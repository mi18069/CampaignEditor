namespace Database.DTOs.SpotPairDTO
{
    public class BaseSpotPairDTO
    {
        public BaseSpotPairDTO(int cmpid, string spotcode, int spotnum)
        {
            this.cmpid = cmpid;
            this.spotcode = spotcode;
            this.spotnum = spotnum;
        }

        public int cmpid { get; set; }
        public string spotcode { get; set; }
        public int spotnum { get; set; }
    }
}
