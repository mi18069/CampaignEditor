
namespace Database.DTOs.SectablesDTO
{
    public class SectablesDTO : BaseIdentitySectablesDTO
    {
        public SectablesDTO(int sctid, int sec, double coef) 
            : base(sctid, sec, coef)
        {
        }
    }
}
