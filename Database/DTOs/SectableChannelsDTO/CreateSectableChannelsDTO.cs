
namespace Database.DTOs.SectableChannels
{
    public class CreateSectableChannelsDTO : BaseIdentitySectableChannelsDTO
    {
        public CreateSectableChannelsDTO(int sctid, int chid) 
            : base(sctid, chid)
        {
        }
    }
}
