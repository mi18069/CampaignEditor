
namespace Database.DTOs.TargetCmpDTO
{
    public class UpdateTargetCmpDTO : BaseIdentityTargetCmpDTO
    {
        public UpdateTargetCmpDTO(int cmpid, int targid, int priority) 
            : base(cmpid, targid, priority)
        {
        }
    }
}
