namespace Database.DTOs.ClientProgCoefDTO
{
    public class ClientProgCoefDTO
    {
        public ClientProgCoefDTO(int clid, int schid, decimal progcoef)
        {
            this.clid = clid;
            this.schid = schid;
            this.progcoef = progcoef;
        }

        public int clid { get; set; }
        public int schid { get; set; }
        public decimal progcoef { get; set; }
    }
}
