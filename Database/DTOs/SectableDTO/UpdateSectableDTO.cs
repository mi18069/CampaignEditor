
namespace Database.DTOs.SectableDTO
{
    public class UpdateSectableDTO : BaseIdentitySectableDTO
    {
        public UpdateSectableDTO(int sctid, string sctname, bool sctlinear, bool sctactive, int ownedby) 
            : base(sctid, sctname, sctlinear, sctactive, ownedby)
        {
        }
    }
}
