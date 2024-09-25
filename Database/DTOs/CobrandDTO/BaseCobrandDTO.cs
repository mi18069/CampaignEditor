namespace Database.DTOs.CobrandDTO
{
    public class BaseCobrandDTO
    {
        public BaseCobrandDTO(int cmpid, int chid, char spotcode, decimal coef)
        {
            this.cmpid = cmpid;
            this.chid = chid;
            this.spotcode = spotcode;
            this.coef = coef;
        }

        public int cmpid { get; set; }
        public int chid { get; set; }
        public char spotcode { get; set; }
        public decimal coef { get; set; }
    }
}
