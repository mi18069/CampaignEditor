
namespace Database.DTOs.TargetDTO
{
    public class UpdateTargetDTO : BaseIdentityTargetDTO
    {
        public UpdateTargetDTO(int targid, string targname, int targown, string targdesc, string targdefi, string targdefp) 
            : base(targid, targname, targown, targdesc, targdefi, targdefp)
        {
        }
    }
}
