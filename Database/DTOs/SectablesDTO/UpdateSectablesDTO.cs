
namespace Database.DTOs.SectablesDTO
{
    public class UpdateSectablesDTO : BaseIdentitySectablesDTO
    {
        public UpdateSectablesDTO(int sctid, int sec, decimal coef) 
            : base(sctid, sec, coef)
        {
        }
    }
}
