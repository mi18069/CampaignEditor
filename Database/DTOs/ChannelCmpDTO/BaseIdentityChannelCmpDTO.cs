namespace Database.DTOs.ChannelCmpDTO
{
    public class BaseIdentityChannelCmpDTO
    {
        public BaseIdentityChannelCmpDTO(int cmpid, int chid, int plid, int actid, int plidbuy, int actidbuy, int pos)
        {
            this.cmpid = cmpid;
            this.chid = chid;
            this.plid = plid;
            this.actid = actid;
            this.plidbuy = plidbuy;
            this.actidbuy = actidbuy;
            this.pos = pos;
        }

        public int cmpid { get; set; }
        public int chid { get; set; }
        public int plid { get; set; }
        public int actid { get; set; }
        public int plidbuy { get; set; }
        public int actidbuy { get; set; }
        public int pos { get; set; }
    }
}
