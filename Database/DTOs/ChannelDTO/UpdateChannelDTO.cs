
namespace Database.DTOs.ChannelDTO
{
    public class UpdateChannelDTO : BaseIdentityChannelDTO
    {
        public UpdateChannelDTO(int chid, bool chactive, int chrdsid, string chsname, int shid, int chrfid) 
            : base(chid, chactive, chrdsid, chsname, shid, chrfid)
        {
        }
    }
}
