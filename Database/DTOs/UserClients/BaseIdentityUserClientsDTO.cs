namespace Database.DTOs.UserClients
{
    public class BaseIdentityUserClientsDTO : BaseUserClientsDTO
    {
        public BaseIdentityUserClientsDTO(int cliid, int usrid, int usrlevel)
        {
            this.cliid = cliid;
            this.usrid = usrid;
            this.usrlevel = usrlevel;
        }

        public int cliid { get; set; }
        public int usrid { get; set; }
        public int usrlevel { get; set; }
    }
}
