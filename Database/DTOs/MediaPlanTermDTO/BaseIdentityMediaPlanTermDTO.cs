﻿using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class BaseIdentityMediaPlanTermDTO : BaseMediaPlanTermDTO
    {
        public BaseIdentityMediaPlanTermDTO(int xmptermid, int xmpid, DateOnly date, string spotcode) : base(xmpid, date, spotcode)
        {
            this.xmptermid = xmptermid;
        }
        public int xmptermid { get; set; }

    }
}
