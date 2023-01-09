namespace Database.DTOs.ClientDTO
{
    public class ClientDTO : BaseIdentityClientDTO
    {
        public ClientDTO(string clname, bool clactive, int spid) 
            : base(clname, clactive, spid)
        {
        }
    }
}
