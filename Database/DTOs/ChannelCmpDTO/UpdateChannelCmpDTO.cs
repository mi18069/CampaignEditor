namespace Database.DTOs.ChannelCmpDTO
{
    public class UpdateChannelCmpDTO : BaseIdentityChannelCmpDTO
    {
        public UpdateChannelCmpDTO(int cmpid, int chid, int plid, int actid, int plidbuy, int actidbuy) 
            : base(cmpid, chid, plid, actid, plidbuy, actidbuy)
        {
        }
    }
}
