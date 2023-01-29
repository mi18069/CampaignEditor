
namespace Database.DTOs.ClientDTO
{
    public class UpdateClientDTO : BaseIdentityClientDTO
    {
        public UpdateClientDTO(string clname, bool clactive, int spid) 
            : base(clname, clactive, spid)
        {
        }
    }
}
