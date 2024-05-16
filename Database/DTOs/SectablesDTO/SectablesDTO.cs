
namespace Database.DTOs.SectablesDTO
{
    public class SectablesDTO : BaseIdentitySectablesDTO
    {
        public SectablesDTO(int sctid, int sec, decimal coef) 
            : base(sctid, sec, coef)
        {
        }
    }
}
