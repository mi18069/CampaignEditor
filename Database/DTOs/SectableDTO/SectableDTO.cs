
namespace Database.DTOs.SectableDTO
{
    public class SectableDTO : BaseIdentitySectableDTO
    {
        public SectableDTO(int sctid, string sctname, bool sctlinear, bool sctactive, string channel, int ownedby) 
            : base(sctid, sctname, sctlinear, sctactive, channel, ownedby)
        {
        }
    }
}
