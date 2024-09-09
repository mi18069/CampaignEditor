namespace Database.Entities
{
    public class ClientRealizedCoefs
    {
        public ClientRealizedCoefs(int clid, int emsnum, decimal progcoef)
        {
            this.clid = clid;
            this.emsnum = emsnum;
            this.progcoef = progcoef;
        }

        public int clid {  get; set; }
        public int emsnum { get; set; }
        public decimal progcoef {  get; set; }
    }
}
