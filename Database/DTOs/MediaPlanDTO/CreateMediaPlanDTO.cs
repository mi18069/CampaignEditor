﻿using System;

namespace Database.DTOs.MediaPlanDTO
{
    public class CreateMediaPlanDTO : BaseMediaPlanDTO
    {
        public CreateMediaPlanDTO(int schid, int cmpid, int chid, string name, int version, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, float progcoef, DateOnly created, DateOnly? modified, double amr1, int amr1trim, double amr2, int amr2trim, double amr3, int amr3trim, double amrsale, int amrsaletrim, double amrp1, double amrp2, double amrp3, double amrpsale, double dpcoef, double seascoef, double price, bool active)
            : base(schid, cmpid, chid, name, version, position, stime, etime, blocktime, days, type, special, sdate, edate, progcoef, created, modified, amr1, amr1trim, amr2, amr2trim, amr3, amr3trim, amrsale, amrsaletrim, amrp1, amrp2, amrp3, amrpsale, dpcoef, seascoef, price, active)
        {
        }

        public CreateMediaPlanDTO(MediaPlanDTO mediaPlanDTO)
            : base(mediaPlanDTO.schid, mediaPlanDTO.cmpid, mediaPlanDTO.chid, mediaPlanDTO.name, mediaPlanDTO.version, mediaPlanDTO.position, mediaPlanDTO.stime, mediaPlanDTO.etime, mediaPlanDTO.blocktime, mediaPlanDTO.days, mediaPlanDTO.type, mediaPlanDTO.special, mediaPlanDTO.sdate, mediaPlanDTO.edate, mediaPlanDTO.progcoef, mediaPlanDTO.created, mediaPlanDTO.modified,
                  mediaPlanDTO.amr1, mediaPlanDTO.amr1trim, mediaPlanDTO.amr2, mediaPlanDTO.amr2trim, mediaPlanDTO.amr3, mediaPlanDTO.amr3trim, mediaPlanDTO.amrsale, mediaPlanDTO.amrsaletrim, mediaPlanDTO.amrp1, mediaPlanDTO.amrp2, mediaPlanDTO.amrp3, mediaPlanDTO.amrpsale, mediaPlanDTO.dpcoef, mediaPlanDTO.seascoef, mediaPlanDTO.price, mediaPlanDTO.active)

        {
        }
    }
}
