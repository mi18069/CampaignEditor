namespace Database.DTOs.ChannelGroupsDTO
{
    public class CreateChannelGroupsDTO : BaseIdentityChannelGroupsDTO
    {
        public CreateChannelGroupsDTO(int chgrid, int chid) 
            : base(chgrid, chid)
        {
        }
    }
}
