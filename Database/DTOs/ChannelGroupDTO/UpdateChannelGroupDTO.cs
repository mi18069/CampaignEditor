namespace Database.DTOs.ChannelGroupDTO
{
    public class UpdateChannelGroupDTO : BaseIdentityChannelGroupDTO
    {
        public UpdateChannelGroupDTO(int chgrid, string chgrname, int chgrown) 
            : base(chgrid, chgrname, chgrown)
        {
        }
    }
}
