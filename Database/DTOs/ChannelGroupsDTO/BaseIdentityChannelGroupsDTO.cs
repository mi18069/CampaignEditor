namespace Database.DTOs.ChannelGroupsDTO
{
    public class BaseIdentityChannelGroupsDTO
    {
        public BaseIdentityChannelGroupsDTO(int chgrid, int chid)
        {
            this.chgrid = chgrid;
            this.chid = chid;
        }

        public int chgrid { get; set; }
        public int chid { get; set; }
    }
}
