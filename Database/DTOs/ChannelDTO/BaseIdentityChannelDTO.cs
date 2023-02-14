
namespace Database.DTOs.ChannelDTO
{
    public class BaseIdentityChannelDTO : BaseChannelDTO
    {
        public BaseIdentityChannelDTO(int chid, bool chactive, int chrdsid, string chsname, int shid, int chrfid) 
            : base(chactive, chrdsid, chsname, shid, chrfid)
        {
            this.chid = chid;
        }

        public int chid { get; set; }
    }
}
