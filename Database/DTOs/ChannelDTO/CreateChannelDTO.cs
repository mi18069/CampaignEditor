
namespace Database.DTOs.ChannelDTO
{
    public class CreateChannelDTO : BaseChannelDTO
    {
        public CreateChannelDTO(bool chactive, string chname, int chrdsid, string chsname, int shid, int chrfid) 
            : base(chactive, chname, chrdsid, chsname, shid, chrfid)
        {
        }
    }
}
