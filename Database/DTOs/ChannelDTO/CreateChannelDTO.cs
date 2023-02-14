
namespace Database.DTOs.ChannelDTO
{
    public class CreateChannelDTO : BaseChannelDTO
    {
        public CreateChannelDTO(bool chactive, int chrdsid, string chsname, int shid, int chrfid) 
            : base(chactive, chrdsid, chsname, shid, chrfid)
        {
        }
    }
}
