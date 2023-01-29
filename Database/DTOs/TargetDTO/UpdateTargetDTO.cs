
namespace Database.DTOs.TargetDTO
{
    public class UpdateTargetDTO : BaseIdentityTargetDTO
    {
        public UpdateTargetDTO(string targname, int targown, string targdesc, string targdefi, string targdefp) 
            : base(targname, targown, targdesc, targdefi, targdefp)
        {
        }
    }
}
