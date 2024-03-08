
namespace Database.DTOs.ClientDTO
{
    public class UpdateClientDTO : BaseIdentityClientDTO
    {
        public UpdateClientDTO(int clid, string clname, bool clactive, int spid) 
            : base(clid, clname, clactive, spid)
        {
        }

        public UpdateClientDTO(ClientDTO client)
            : base(client.clid, client.clname, client.clactive, client.spid)
        {
        }
    }
}
