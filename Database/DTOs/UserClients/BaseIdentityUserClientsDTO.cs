namespace Database.DTOs.UserClients
{
    public class BaseIdentityUserClientsDTO : BaseUserClientsDTO
    {
        public BaseIdentityUserClientsDTO(int cliid, int usrid)
        {
            this.cliid = cliid;
            this.usrid = usrid;
        }

        public int cliid { get; set; }
        public int usrid { get; set; }
    }
}
