namespace Database.DTOs.ClientCoefsDTO
{
    public class ClientCoefsDTO
    {
        public ClientCoefsDTO(int clid, int schid, decimal? progcoef, decimal? coefA, decimal? coefB)
        {
            this.clid = clid;
            this.schid = schid;
            this.progcoef = progcoef;
            this.coefA = coefA;
            this.coefB = coefB;
        }

        public int clid { get; set; }
        public int schid { get; set; }
        public decimal? progcoef { get; set; }
        public decimal? coefA { get; set; }
        public decimal? coefB { get; set; }
    }
}
