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
                "@Days, @Type, @Special, CAST(@Sdate AS DATE), CAST(@Edate AS DATE), " +
                "@Progcoef, CAST(@Created AS DATE), CAST(@Modified AS DATE))",
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
                Sdate = schemaDTO.sdate.ToString("yyyy-MM-dd"),
                Edate = schemaDTO.edate?.ToString("yyyy-MM-dd"),
                Progcoef = schemaDTO.progcoef,
                Created = schemaDTO.created.ToString("yyyy-MM-dd"),
                Modified = schemaDTO.modified?.ToString("yyyy-MM-dd")
            });


            return affected != 0;
        }

        public async Task<SchemaDTO> CreateGetSchema(CreateSchemaDTO schemaDTO)
        {
            using var connection = _context.GetConnection();

            // Check if the same schema exists
            var existingSchema = await connection.QueryAsync<dynamic>(
            "SELECT * FROM progschema WHERE chid = @Chid AND naziv = @Name AND pozicija = @Position AND " +
            "vremeod = @Stime AND (vremedo = @Etime OR vremedo IS NULL AND @Etime IS NULL) AND (vremerbl = @Blocktime OR vremerbl IS NULL AND @Blocktime IS NULL) AND " +
            "dani = @Days AND tipologija = @Type AND specijal = @Special AND " +
            "datumod = CAST(@Sdate as DATE) AND (datumdo = CAST(@Edate as DATE) OR datumdo IS NULL AND CAST(@Edate AS DATE) IS NULL) AND " +
            "progkoef = @Progcoef ",
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
                Sdate = schemaDTO.sdate.ToString("yyyy-MM-dd"),
                Edate = schemaDTO.edate?.ToString("yyyy-MM-dd"),
                Progcoef = schemaDTO.progcoef
            });

            // If schema exists, return it
            if (existingSchema.Count() != 0)
            {
                existingSchema = existingSchema.Select(item => new Schema()
                {
                    id = item.id,
                    chid = item.chid,
                    name = item.naziv,
                    position = item.pozicija,
                    stime = item.vremeod,
                    etime = item.vremedo,
                    blocktime = item.vremerbl,
                    days = item.dani,
                    type = item.tiopologija,
                    special = item.specijal,
                    sdate = DateOnly.FromDateTime(item.datumod),
                    edate = item.datumdo == null ? null : DateOnly.FromDateTime(item.datumdo),
                    progcoef = (float)item.progkoef,
                    created = DateOnly.FromDateTime(item.datumkreiranja),
                    modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene)
                });

                return _mapper.Map<SchemaDTO>(existingSchema.FirstOrDefault());
            }
            // If schema doesn't exist
            else
            {
                var id = -1;
                // Try to make new schema
                try
                {
                id = await connection.ExecuteScalarAsync<int>(
                "INSERT INTO progschema (chid, naziv, pozicija, vremeod, vremedo, vremerbl, dani, tipologija, specijal, datumod, datumdo, progkoef, datumkreiranja, datumizmene) " +
                "VALUES (@Chid, @Name, @Position, @Stime, @Etime, @Blocktime, @Days, @Type, @Special, " +
                "CAST(@Sdate AS DATE), CAST(@Edate AS DATE), @Progcoef, CAST(@Created AS DATE), CAST(@Modified AS DATE)) " +
                "RETURNING id",
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
                    Sdate = schemaDTO.sdate.ToString("yyyy-MM-dd"),
                    Edate = schemaDTO.edate?.ToString("yyyy-MM-dd"),
                    Progcoef = schemaDTO.progcoef,
                    Created = schemaDTO.created.ToString("yyyy-MM-dd"),
                    Modified = schemaDTO.modified?.ToString("yyyy-MM-dd")
                });                 
                }
                // If key constraints don't allow to create schema, update the one that's making conflict
                catch
                {
                    id = await connection.ExecuteScalarAsync<int>(
                    "UPDATE progschema SET chid = @Chid, naziv = @Name, pozicija = @Position, " +
                    "vremeod = @Stime, vremedo = @Etime, vremerbl = @Blocktime, dani = @Days, " +
                    "tipologija = @Type, specijal = @Special, datumod = CAST(@Sdate AS DATE), datumdo = CAST(@Edate AS DATE), " +
                    "progkoef = @Progcoef, datumizmene = CAST(@NewModified AS DATE) " +
                    "WHERE chid = @Chid AND pozicija = @Position AND " +
                    "vremeod = @Stime AND (vremedo = @Etime OR vremedo IS NULL AND @Etime IS NULL) AND (vremerbl = @Blocktime OR vremerbl IS NULL AND @Blocktime IS NULL) AND " +
                    "dani = @Days AND tipologija = @Type AND specijal = @Special AND " +
                    "datumod = CAST(@Sdate as DATE) AND " +
                    "(datumdo = CAST(@Edate as DATE) OR datumdo IS NULL AND CAST(@Edate AS DATE) IS NULL) AND " +
                    "progkoef = @Progcoef " +
                    "RETURNING id",
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
                        Sdate = schemaDTO.sdate.ToString("yyyy-MM-dd"),
                        Edate = schemaDTO.edate?.ToString("yyyy-MM-dd"),
                        Progcoef = schemaDTO.progcoef,
                        NewModified = DateTime.Now.ToString("yyyy-MM-dd")
                    });                  
                }
                // Fetch the newly created record from the table and return it
                return await GetSchemaById(id);

            }
     
        }

        public async Task<SchemaDTO> GetSchemaById(int id)
        {
            using var connection = _context.GetConnection();

            var schema = await connection.QueryAsync<dynamic>(
                "SELECT * FROM progschema WHERE id = @Id", new { Id = id });

            schema = schema.Select(item => new Schema()
            {
                id = item.id,
                chid = item.chid,
                name = item.naziv,
                position = item.pozicija,
                stime = item.vremeod,
                etime = item.vremedo,
                blocktime = item.vremerbl,
                days = item.dani,
                type = item.tipologija,
                special = item.specijal,
                sdate = DateOnly.FromDateTime(item.datumod),
                edate = item.datumdo == null ? null : DateOnly.FromDateTime(item.datumdo),
                progcoef = (float)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene)
            });

            return _mapper.Map<SchemaDTO>(schema.FirstOrDefault());
        }

        public async Task<SchemaDTO> GetSchemaByName(string name)
        {
            using var connection = _context.GetConnection();

            var schema = await connection.QueryAsync<dynamic>(
                "SELECT * FROM progschema WHERE naziv = @Name", new { Name = name });

            schema = schema.Select(item => new Schema()
            {
                id = item.id,
                chid = item.chid,
                name = item.naziv,
                position = item.pozicija,
                stime = item.vremeod,
                etime = item.vremedo,
                blocktime = item.vremerbl,
                days = item.dani,
                type = item.tiopologija,
                special = item.specijal,
                sdate = DateOnly.FromDateTime(item.datumod),
                edate = item.datumdo == null ? null : DateOnly.FromDateTime(item.datumdo),
                progcoef = (float)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene)
            });

            return _mapper.Map<SchemaDTO>(schema.FirstOrDefault());
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllChannelSchemas(int chid)
        {
            using var connection = _context.GetConnection();

            var allSchemas = await connection.QueryAsync<dynamic>
                ("SELECT * FROM progschema WHERE chid = @Chid", new { Chid = chid });

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

            var allSchemas = await connection.QueryAsync<dynamic>
                ("SELECT * FROM progschema");

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

        public async Task<bool> UpdateSchema(UpdateSchemaDTO schemaDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE progschema SET id = @Id, chid = @Chid, naziv = @Name, pozicija = @Position, " +
                "vremeod = @Stime, vremedo = @Etime, vremerbl = @Blocktime, dani = @Days, " +
                "tipologija = @Type, specijal = @Special, datumod = CAST(@Sdate AS DATE), datumdo = CAST(@Edate AS DATE), " +
                "progkoef = @Progcoef, datumkreiranja = CAST(@Created AS DATE), datumizmene = CAST(@Modified AS DATE) " +
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
                    Sdate = schemaDTO.sdate.ToString("yyyy-MM-dd"),
                    Edate = schemaDTO.edate?.ToString("yyyy-MM-dd"),
                    Progcoef = schemaDTO.progcoef,
                    Created = schemaDTO.created.ToString("yyyy-MM-dd"),
                    Modified = schemaDTO.modified?.ToString("yyyy-MM-dd")
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
