namespace Database.DTOs.ChannelGroupDTO
{
    public class ChannelGroupDTO : BaseIdentityChannelGroupDTO
    {
        public ChannelGroupDTO(int chrgid, string chgrname, int chgrown) 
            : base(chrgid, chgrname, chgrown)
        {
        }
    }
}
