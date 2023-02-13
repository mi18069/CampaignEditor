
namespace Database.DTOs.TargetDTO
{
    public class BaseIdentityTargetDTO : BaseTargetDTO
    {
        public BaseIdentityTargetDTO(int targid, string targname, int targown, string targdesc, string targdefi, string targdefp) 
            : base(targname, targown, targdesc, targdefi, targdefp)
        {
            this.targid = targid;
        }

        public int targid { get; set; }
    }  
}
