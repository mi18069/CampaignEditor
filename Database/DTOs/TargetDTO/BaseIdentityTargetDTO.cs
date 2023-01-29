
namespace Database.DTOs.TargetDTO
{
    public class BaseIdentityTargetDTO : BaseTargetDTO
    {
        public BaseIdentityTargetDTO(string targname, int targown, string targdesc, string targdefi, string targdefp) 
            : base(targname, targown, targdesc, targdefi, targdefp)
        {
        }

        public int targid { get; set; }
    }  
}
