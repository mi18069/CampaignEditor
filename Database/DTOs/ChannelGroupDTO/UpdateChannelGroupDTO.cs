namespace Database.DTOs.ChannelGroupDTO
{
    public class UpdateChannelGroupDTO : BaseIdentityChannelGroupDTO
    {
        public UpdateChannelGroupDTO(int chrgid, string chgrname, int chgrown) 
            : base(chrgid, chgrname, chgrown)
        {
        }
    }
}
