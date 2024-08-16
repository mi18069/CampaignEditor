namespace Database.DTOs.ChannelCmpDTO
{
    public class ChannelCmpDTO : BaseIdentityChannelCmpDTO
    {
        public ChannelCmpDTO(int cmpid, int chid, int plid, int actid, int plidbuy, int actidbuy, int pos) 
            : base(cmpid, chid, plid, actid, plidbuy, actidbuy, pos)
        {
        }
    }
}
