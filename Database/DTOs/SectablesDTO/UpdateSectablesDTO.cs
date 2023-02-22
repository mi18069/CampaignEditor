
namespace Database.DTOs.SectablesDTO
{
    public class UpdateSectablesDTO : BaseIdentitySectablesDTO
    {
        public UpdateSectablesDTO(int sctid, int sec, double coef) 
            : base(sctid, sec, coef)
        {
        }
    }
}
