﻿namespace Database.DTOs.ChannelGroupDTO
{
    public class BaseIdentityChannelGroupDTO : BaseChannelGroupDTO
    {
        public BaseIdentityChannelGroupDTO(int chgrid, string chgrname, int chgrown) 
            : base(chgrname, chgrown)
        {
            this.chgrid = chgrid;
        }

        public int chgrid { get; set; }

    }
}
