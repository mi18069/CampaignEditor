﻿using System;

namespace Database.DTOs.SchemaDTO
{
    public class BaseSchemaDTO
    {
        public BaseSchemaDTO(int chid, string name, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, decimal progcoef, DateOnly created, DateOnly? modified)
        {
            this.chid = chid;
            this.name = name;
            this.position = position;
            this.stime = stime;
            this.etime = etime;
            this.blocktime = blocktime;
            this.days = days;
            this.type = type;
            this.special = special;
            this.sdate = sdate;
            this.edate = edate;
            this.progcoef = progcoef;
            this.created = created;
            this.modified = modified;
        }

        public int chid { get; set; }
        public string name { get; set; }
        public string position { get; set; }
        public string stime { get; set; }
        public string? etime { get; set; }
        public string? blocktime { get; set; }
        public string days { get; set; }
        public string type { get; set; }
        public bool special { get; set; }
        public DateOnly sdate { get; set; }
        public DateOnly? edate { get; set; }
        public decimal progcoef { get; set; }
        public DateOnly created { get; set; }
        public DateOnly? modified { get; set; }
    }
}
