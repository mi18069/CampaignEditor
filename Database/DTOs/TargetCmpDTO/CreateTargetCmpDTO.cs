
namespace Database.DTOs.TargetCmpDTO
{
    public class CreateTargetCmpDTO : BaseIdentityTargetCmpDTO
    {
        public CreateTargetCmpDTO(int cmpid, int targid, int priority) 
            : base(cmpid, targid, priority)
        {
        }

        public CreateTargetCmpDTO(TargetCmpDTO targetCmp)
            : base(targetCmp.cmpid, targetCmp.targid, targetCmp.priority)
        {
        }
    }
}
