using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.SchemaDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SchemaRepository : ISchemaRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public SchemaRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateSchema(CreateSchemaDTO schemaDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO progschema (chid, name, position, stime, etime, blocktime, " +
                "days, type, special, sdate, edate, progcoef, created, modified) " +
                "VALUES (@Chid, @Name, @Position, @Stime, @Etime, @Blocktime, " +
                "@Days, @Type, @Special, @Sdate, @Edate, @Progcoef, @Created, @Modified)",
            new
            {
                Name = schemaDTO.name,
                Position = schemaDTO.position,
                Stime = schemaDTO.stime,
                Etime = schemaDTO.etime,
                Blocktime = schemaDTO.blocktime,
                Days = schemaDTO.days,
                Type = schemaDTO.type,
                Special = schemaDTO.special,
                Sdate = schemaDTO.sdate,
                Edate = schemaDTO.edate,
                Progcoef = schemaDTO.progcoef,
                Created = schemaDTO.created,
                Modified = schemaDTO.modified
            });


            return affected != 0;
        }

        public async Task<SchemaDTO> GetSchemaById(int id)
        {
            using var connection = _context.GetConnection();

            var schema = await connection.QueryFirstOrDefaultAsync<Schema>(
                "SELECT * FROM progschema WHERE id = @Id", new { Id = id });

            return _mapper.Map<SchemaDTO>(schema);
        }

        public async Task<SchemaDTO> GetSchemaByName(string name)
        {
            using var connection = _context.GetConnection();

            var schema = await connection.QueryFirstOrDefaultAsync<Schema>(
                "SELECT * FROM progschema WHERE name = @Name", new { Name = name });

            return _mapper.Map<SchemaDTO>(schema);
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllChannelSchemas(int chid)
        {
            using var connection = _context.GetConnection();

            var allSchemas = await connection.QueryAsync<Schema>
                ("SELECT * FROM progschema WHERE chid = @Chid", new { Chid = chid });

            return _mapper.Map<IEnumerable<SchemaDTO>>(allSchemas);
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllSchemas()
        {
            using var connection = _context.GetConnection();

            var allSchemas = await connection.QueryAsync<Schema>
                ("SELECT * FROM progschema");

            return _mapper.Map<IEnumerable<SchemaDTO>>(allSchemas);
        }

        public async Task<bool> UpdateSchema(UpdateSchemaDTO schemaDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE progschema SET id = @Id, name = @Name, position = @Position, " +
                "stime = @Stime, etime = @Etime, blocktime = @Blocktime, days = @Days, " +
                "Type = @Type, special = @Special, sdate = @Sdate, edate = @Edate, " +
                "created = @Created, modified = @Modified " +
                "WHERE id = @Id",
                new
                {
                    Id = schemaDTO.id,
                    Name = schemaDTO.name,
                    Position = schemaDTO.position,
                    Stime = schemaDTO.stime,
                    Etime = schemaDTO.etime,
                    Blocktime = schemaDTO.blocktime,
                    Days = schemaDTO.days,
                    Type = schemaDTO.type,
                    Special = schemaDTO.special,
                    Sdate = schemaDTO.sdate,
                    Edate = schemaDTO.edate,
                    Progcoef = schemaDTO.progcoef,
                    Created = schemaDTO.created,
                    Modified = schemaDTO.modified
                });

            return affected != 0;
        }

        public async Task<bool> DeleteSchemaById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM progschema WHERE id = @Id", new { Id = id });

            return affected != 0;
        }
    }
}
