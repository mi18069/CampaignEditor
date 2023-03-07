
namespace Database.DTOs.SectableChannels
{
    public class UpdateSectableChannelsDTO : BaseIdentitySectableChannelsDTO
    {
        public UpdateSectableChannelsDTO(int sctid, int chid) 
            : base(sctid, chid)
        {
        }
    }
}
