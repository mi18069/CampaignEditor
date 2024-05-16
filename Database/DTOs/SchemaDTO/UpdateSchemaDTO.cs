﻿using System;

namespace Database.DTOs.SchemaDTO
{
    public class UpdateSchemaDTO : BaseIdentitySchemaDTO
    {
        public UpdateSchemaDTO(int id, int chid, string name, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, decimal progcoef, DateOnly created, DateOnly? modified) 
            : base(id, chid, name, position, stime, etime, blocktime, days, type, special, sdate, edate, progcoef, created, modified)
        {
        }

        public UpdateSchemaDTO(SchemaDTO schemaDTO)
            : base(schemaDTO.id, schemaDTO.chid, schemaDTO.name, schemaDTO.position, schemaDTO.stime, schemaDTO.etime, schemaDTO.blocktime, schemaDTO.days, schemaDTO.type, schemaDTO.special, schemaDTO.sdate, schemaDTO.edate, schemaDTO.progcoef, schemaDTO.created, schemaDTO.modified)
        {
        }
    }
}
