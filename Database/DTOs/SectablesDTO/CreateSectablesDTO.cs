
namespace Database.DTOs.SectablesDTO
{
    public class CreateSectablesDTO : BaseIdentitySectablesDTO
    {
        public CreateSectablesDTO(int sctid, int sec, decimal coef) 
            : base(sctid, sec, coef)
        {
        }
    }
}
