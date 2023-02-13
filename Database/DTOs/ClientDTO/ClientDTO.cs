namespace Database.DTOs.ClientDTO
{
    public class ClientDTO : BaseIdentityClientDTO
    {
        public ClientDTO(int clid, string clname, bool clactive, int spid) 
            : base(clid, clname, clactive, spid)
        {
        }
    }
}
