
namespace Database.DTOs.SectableDTO
{
    public class BaseIdentitySectableDTO : BaseSectableDTO
    {
        public BaseIdentitySectableDTO(int sctid, string sctname, bool sctlinear, bool sctactive, int ownedby) 
            : base(sctname, sctlinear, sctactive, ownedby)
        {
            this.sctid = sctid;
        }
        public int sctid { get; set; }
    }
}
