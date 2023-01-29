
namespace Database.DTOs.ClientDTO
{
    public class CreateClientDTO : BaseClientDTO
    {
        public CreateClientDTO(string clname, bool clactive, int spid) 
            : base(clname, clactive, spid)
        {
        }
    }
}
