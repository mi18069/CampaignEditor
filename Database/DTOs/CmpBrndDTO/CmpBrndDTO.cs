namespace Database.DTOs.CmpBrndDTO
{
    public class CmpBrndDTO
    {
        public CmpBrndDTO(int cmpid, int brbrand)
        {
            this.cmpid = cmpid;
            this.brbrand = brbrand;
        }

        public int cmpid { get; set; }
        public int brbrand { get; set; }
    }
}
