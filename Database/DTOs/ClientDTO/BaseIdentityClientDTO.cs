
namespace Database.DTOs.ClientDTO
{
    public class BaseIdentityClientDTO : BaseClientDTO
    {
        public BaseIdentityClientDTO(int clid, string clname, bool clactive, int spid) 
            : base(clname, clactive, spid)
        {
            this.clid = clid;
        }
        public int clid { get; set; }
    }
}
