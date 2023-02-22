
namespace Database.DTOs.SectableDTO
{
    public class CreateSectableDTO : BaseSectableDTO
    {
        public CreateSectableDTO(string sctname, bool sctlinear, bool sctactive, string channel, int ownedby) 
            : base(sctname, sctlinear, sctactive, channel, ownedby)
        {
        }
    }
}
