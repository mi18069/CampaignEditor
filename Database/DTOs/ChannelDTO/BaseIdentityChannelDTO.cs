
namespace Database.DTOs.ChannelDTO
{
    public class BaseIdentityChannelDTO : BaseChannelDTO
    {
        public BaseIdentityChannelDTO(int chid, string chname, bool chactive, int chrdsid, string chsname, int shid, int chrfid) 
            : base(chactive, chname, chrdsid, chsname, shid, chrfid)
        {
            this.chid = chid;
        }

        public int chid { get; set; }
    }
}
