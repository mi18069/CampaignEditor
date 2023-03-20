namespace Database.DTOs.ChannelGroupDTO
{
    public class CreateChannelGroupDTO : BaseChannelGroupDTO
    {
        public CreateChannelGroupDTO(string chgrname, int chgrown) 
            : base(chgrname, chgrown)
        {
        }
    }
}
