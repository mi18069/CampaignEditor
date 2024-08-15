namespace Database.Entities
{
    public class DGConfig
    {
        public DGConfig(int usrid, int clid, string dgfor, string dgexp, string dgreal)
        {
            this.usrid = usrid;
            this.clid = clid;
            this.dgfor = dgfor;
            this.dgexp = dgexp;
            this.dgreal = dgreal;
        }

        public int usrid { get; set; }
        public int clid { get; set; }
        public string dgfor { get; set; }
        public string dgexp { get; set; }
        public string dgreal { get; set; }

    }
}
