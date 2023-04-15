using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.SchemaDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
                "INSERT INTO progschema (chid, naziv, pozicija, vremeod, vremedo, vremerbl, " +
                "dani, tipologija, specijal, datumod, datumdo, progkoef, datumkreiranja, datumizmene) " +
                "VALUES (@Chid, @Name, @Position, @Stime, @Etime, @Blocktime, " +
                "@Days, @Type, @Special, @Sdate, @Edate, @Progcoef, @Created, @Modified)",
            new
            {
                Chid = schemaDTO.chid,
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
                "SELECT * FROM progschema WHERE naziv = @Name", new { Name = name });

            return _mapper.Map<SchemaDTO>(schema);
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllChannelSchemas(int chid)
        {
            using var connection = _context.GetConnection();

            var allSchemas = await connection.QueryAsync<Schema>
                ("SELECT * FROM progschema WHERE chid = @Chid", new { Chid = chid });

            return _mapper.Map<IEnumerable<SchemaDTO>>(allSchemas);
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllChannelSchemasWithinDate(int chid, DateOnly sdate, DateOnly edate)
        {
            using var connection = _context.GetConnection();

            var allSchemas = await connection.QueryAsync<dynamic>
                ("SELECT * FROM progschema WHERE chid = @Chid AND " +
                "datumod <= CAST ( @Edate AS DATE) AND " +
                "( datumdo >= CAST ( @Sdate as DATE) OR datumdo IS NULL) "
                , new { Chid = chid, Sdate = sdate.ToString("yyyy-MM-dd"), Edate = edate.ToString("yyyy-MM-dd") });
            allSchemas = allSchemas.Select(item => new Schema()
            {
                id = item.id,
                chid = item.chid,
                name = item.naziv,
                position = item.pozicija,
                stime = item.vremeod,
                etime = item.vremedo == null ? null : item.vremedo,
                blocktime = item.vremerbl == null ? null : item.vremerbl,
                days = item.dani,
                type = item.tipologija,
                special = item.specijal,
                sdate = DateOnly.FromDateTime(item.datumod),
                edate = item.datumdo == null ? null : DateOnly.FromDateTime(item.datumdo),
                progcoef = (float)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene)
            });

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
                "UPDATE progschema SET id = @Id, chid = @Chid, naziv = @Name, pozicija = @Position, " +
                "vremeod = @Stime, vremedo = @Etime, vremerbl = @Blocktime, dani = @Days, " +
                "tipologija = @Type, specijal = @Special, datumod = @Sdate, datumdo = @Edate, " +
                "datumkreiranja = @Created, datumizmene = @Modified " +
                "WHERE id = @Id",
                new
                {
                    Id = schemaDTO.id,
                    Chid = schemaDTO.chid,
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
