
namespace Database.DTOs.TargetDTO
{
    public class TargetDTO : BaseTargetDTO
    {
        public TargetDTO(string targname, int targown, string targdesc, string targdefi, string targdefp) 
            : base(targname, targown, targdesc, targdefi, targdefp)
        {
        }
    }
}
