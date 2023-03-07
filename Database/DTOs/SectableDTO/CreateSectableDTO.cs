
namespace Database.DTOs.SectableDTO
{
    public class CreateSectableDTO : BaseSectableDTO
    {
        public CreateSectableDTO(string sctname, bool sctlinear, bool sctactive, int ownedby) 
            : base(sctname, sctlinear, sctactive, ownedby)
        {
        }
    }
}
