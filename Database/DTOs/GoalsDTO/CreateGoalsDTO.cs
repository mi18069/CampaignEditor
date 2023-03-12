
namespace Database.DTOs.GoalsDTO
{
    public class CreateGoalsDTO : BaseIdentityGoalsDTO
    {
        public CreateGoalsDTO(int cmpid, int budget, int grp, int ins, int rch_f1, int rch_f2, int rch) 
            : base(cmpid, budget, grp, ins, rch_f1, rch_f2, rch)
        {
        }
    }
}
