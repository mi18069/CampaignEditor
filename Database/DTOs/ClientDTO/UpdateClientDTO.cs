
namespace Database.DTOs.ClientDTO
{
    public class UpdateClientDTO : BaseIdentityClientDTO
    {
        public UpdateClientDTO(int clid, string clname, bool clactive, int spid) 
            : base(clid, clname, clactive, spid)
        {
        }
    }
}
