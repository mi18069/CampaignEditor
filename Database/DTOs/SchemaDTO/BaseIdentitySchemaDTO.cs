using System;

namespace Database.DTOs.SchemaDTO
{
    public class BaseIdentitySchemaDTO : BaseSchemaDTO
    {
        public BaseIdentitySchemaDTO(int id, int chid, string name, string position, string stime, string etime, string blocktime, string days, string type, bool special, DateOnly sdate, DateOnly edate, float progcoef, DateOnly created, DateOnly modified) 
            : base(chid, name, position, stime, etime, blocktime, days, type, special, sdate, edate, progcoef, created, modified)
        {
            this.id = id;
        }
        public int id { get; set; }
    }
}
