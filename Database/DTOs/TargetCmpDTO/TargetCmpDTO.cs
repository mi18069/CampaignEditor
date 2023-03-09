
namespace Database.DTOs.TargetCmpDTO
{
    public class TargetCmpDTO : BaseIdentityTargetCmpDTO
    {
        public TargetCmpDTO(int cmpid, int targid, int priority) 
            : base(cmpid, targid, priority)
        {
        }
    }
}
