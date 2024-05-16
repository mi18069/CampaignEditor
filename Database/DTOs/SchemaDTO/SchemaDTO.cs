using System;

namespace Database.DTOs.SchemaDTO
{
    public class SchemaDTO : BaseIdentitySchemaDTO
    {
        public SchemaDTO(int id, int chid, string name, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, decimal progcoef, DateOnly created, DateOnly? modified) 
            : base(id, chid, name, position, stime, etime, blocktime, days, type, special, sdate, edate, progcoef, created, modified)
        {
        }
    }
}
