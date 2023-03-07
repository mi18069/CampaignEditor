
namespace Database.DTOs.SectableChannels
{
    public class BaseIdentitySectableChannelsDTO
    {
        public BaseIdentitySectableChannelsDTO(int sctid, int chid)
        {
            this.sctid = sctid;
            this.chid = chid;
        }

        public int sctid { get; set; }
        public int chid { get; set; }
    }
}
