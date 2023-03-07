
namespace Database.DTOs.ChannelDTO
{
    public class ChannelDTO : BaseIdentityChannelDTO
    {
        public ChannelDTO(int chid, string chname, bool chactive, int chrdsid, string chsname, int shid, int chrfid) 
            : base(chid, chname, chactive, chrdsid, chsname, shid, chrfid)
        {
        }
    }
}
