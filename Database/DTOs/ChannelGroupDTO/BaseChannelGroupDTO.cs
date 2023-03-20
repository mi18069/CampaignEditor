
using System;

namespace Database.DTOs.ChannelGroupDTO
{
    public class BaseChannelGroupDTO
    {
        public BaseChannelGroupDTO(string chgrname, int chgrown)
        {
            this.chgrname = chgrname ?? throw new ArgumentNullException(nameof(chgrname));
            this.chgrown = chgrown;
        }

        public string chgrname { get; set; }
        public int chgrown { get; set; }
    }
}
