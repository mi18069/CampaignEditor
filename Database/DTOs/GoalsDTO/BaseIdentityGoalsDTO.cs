
namespace Database.DTOs.GoalsDTO
{
    public class BaseIdentityGoalsDTO
    {
        public BaseIdentityGoalsDTO(int cmpid, int budget, int grp, int ins, int rch_f1, int rch_f2, int rch)
        {
            this.cmpid = cmpid;
            this.budget = budget;
            this.grp = grp;
            this.ins = ins;
            this.rch_f1 = rch_f1;
            this.rch_f2 = rch_f2;
            this.rch = rch;
        }

        public int cmpid { get; set; }
        public int budget { get; set; }
        public int grp { get; set; }
        public int ins { get; set; }
        public int rch_f1 { get; set; }
        public int rch_f2 { get; set; }
        public int rch { get; set; }
    }
}
