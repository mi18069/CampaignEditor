namespace Database.DTOs.UserClients
{
    public class UserClientsDTO : BaseIdentityUserClientsDTO
    {
        public UserClientsDTO(int cliid, int usrid) 
            : base(cliid, usrid)
        {
        }
    }
}
