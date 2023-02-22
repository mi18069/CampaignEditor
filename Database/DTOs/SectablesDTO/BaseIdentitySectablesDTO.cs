
namespace Database.DTOs.SectablesDTO
{
    public class BaseIdentitySectablesDTO
    {
        public BaseIdentitySectablesDTO(int sctid, int sec, double coef)
        {
            this.sctid = sctid;
            this.sec = sec;
            this.coef = coef;
        }

        public int sctid { get; set; }
        public int sec { get; set; }
        public double coef { get; set; }
    }
}
