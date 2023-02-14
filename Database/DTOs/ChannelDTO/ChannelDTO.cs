
namespace Database.DTOs.ChannelDTO
{
    public class ChannelDTO : BaseIdentityChannelDTO
    {
        public ChannelDTO(int chid, bool chactive, int chrdsid, string chsname, int shid, int chrfid) 
            : base(chid, chactive, chrdsid, chsname, shid, chrfid)
        {
        }
    }
}
