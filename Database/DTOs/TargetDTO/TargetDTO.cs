
namespace Database.DTOs.TargetDTO
{
    public class TargetDTO : BaseIdentityTargetDTO
    {
        public TargetDTO(int targid, string targname, int targown, string targdesc, string targdefi, string targdefp) 
            : base(targid, targname, targown, targdesc, targdefi, targdefp)
        {
        }
    }
}
