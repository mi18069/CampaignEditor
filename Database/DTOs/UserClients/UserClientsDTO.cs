namespace Database.DTOs.UserClients
{
    public class UserClientsDTO : BaseIdentityUserClientsDTO
    {
        public UserClientsDTO(int cliid, int usrid, int usrlevel) 
            : base(cliid, usrid, usrlevel)
        {
        }
    }
}
