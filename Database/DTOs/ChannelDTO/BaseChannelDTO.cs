﻿using System;

namespace Database.DTOs.ChannelDTO
{
    public class BaseChannelDTO
    {
        public BaseChannelDTO(bool chactive, string chname, int chrdsid, string chsname, int shid, int chrfid)
        {
            this.chactive = chactive;
            this.chname = chname ?? throw new ArgumentNullException(nameof(chname));
            this.chrdsid = chrdsid;
            this.chsname = chsname ?? "";
            this.shid = shid;
            this.chrfid = chrfid;
        }

        public bool chactive { get; set; }
        public string chname { get; set; }
        public int chrdsid { get; set; }
        public string chsname { get; set; }
        public int shid { get; set; }
        public int chrfid { get; set; }
    }
}
