
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

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            BaseIdentityChannelDTO other = (BaseIdentityChannelDTO)obj;

            return chid == other.chid;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + chid.GetHashCode();
                return hash;
            }
        }
    }
}
