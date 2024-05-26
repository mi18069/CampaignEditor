
namespace Database.DTOs.ChannelDTO
{
    public class ChannelDTO : BaseIdentityChannelDTO
    {
        public ChannelDTO(int chid, string chname, bool chactive, int chrdsid, string chsname, int shid, int chrfid) 
            : base(chid, chname, chactive, chrdsid, chsname, shid, chrfid)
        {
        }

        // Override Equals to ensure correct comparison when setting SelectedItem
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ChannelDTO other = (ChannelDTO)obj;
            return chid == other.chid;
        }

        public override int GetHashCode()
        {
            return chid.GetHashCode();
        }

    }
}
