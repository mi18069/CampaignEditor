
namespace Database.DTOs.SectablesDTO
{
    public class BaseIdentitySectablesDTO
    {
        public BaseIdentitySectablesDTO(int sctid, int sec, decimal coef)
        {
            this.sctid = sctid;
            this.sec = sec;
            this.coef = coef;
        }

        public int sctid { get; set; }
        public int sec { get; set; }
        public decimal coef { get; set; }
    }
}
