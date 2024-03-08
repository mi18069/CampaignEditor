namespace Database.DTOs.ChannelCmpDTO
{
    public class CreateChannelCmpDTO : BaseIdentityChannelCmpDTO
    {
        public CreateChannelCmpDTO(int cmpid, int chid, int plid, int actid, int plidbuy, int actidbuy) 
            : base(cmpid, chid, plid, actid, plidbuy, actidbuy)
        {
        }

        public CreateChannelCmpDTO(ChannelCmpDTO channelCmp)
            : base(channelCmp.cmpid, channelCmp.chid, channelCmp.plid, channelCmp.actid, channelCmp.plidbuy, channelCmp.actidbuy)
        {
        }
    }
}
