
namespace Database.DTOs.SectableDTO
{
    public class BaseIdentitySectableDTO : BaseSectableDTO
    {
        public BaseIdentitySectableDTO(int sctid, string sctname, bool sctlinear, bool sctactive, string channel, int ownedby) 
            : base(sctname, sctlinear, sctactive, channel, ownedby)
        {
            this.sctid = sctid;
        }
        public int sctid { get; set; }
    }
}
