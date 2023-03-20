namespace Database.DTOs.ChannelGroupDTO
{
    public class ChannelGroupDTO : BaseIdentityChannelGroupDTO
    {
        public ChannelGroupDTO(int chgrid, string chgrname, int chgrown) 
            : base(chgrid, chgrname, chgrown)
        {
        }
    }
}
