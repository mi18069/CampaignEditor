using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class MediaPlanRepository : IMediaPlanRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public MediaPlanRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateMediaPlan(CreateMediaPlanDTO mediaPlanDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO xmp (schid, cmpid, chid, naziv, verzija, pozicija, vremeod, vremedo, vremerbl, " +
                "dani, tipologija, specijal, datumod, datumdo, progkoef, datumkreiranja, datumizmene, " +
                "amr1, amr1trim, amr2, amr2trim, amr3, amr3trim, amrsale, amrsaletrim, amrp1, amrp2, amrp3, amrpsale, dpkoef, seaskoef, price, active) " +
                "VALUES (@Schid, @Cmpid, @Chid, @Name, @Version, @Position, @Stime, @Etime, @Blocktime, " +
                "@Days, @Type, @Special, CAST (@Sdate AS DATE), CAST(@Edate AS DATE), @Progcoef, CAST(@Created AS DATE), CAST(@Modified AS DATE), " +
                "@Amr1, @Amr1trim, @Amr2, @Amr2trim, @Amr3, @Amr3trim, @Amrsale, @Amrsaletrim, @Amrp1, @Amrp2, @Amrp3, @Amrpsale, @Dpcoef, @Seascoef, @Price, @Active) ",
            new
            {
                Schid = mediaPlanDTO.schid,
                Cmpid = mediaPlanDTO.cmpid,
                Chid = mediaPlanDTO.chid,
                Name = mediaPlanDTO.name,
                Version = mediaPlanDTO.version,
                Position = mediaPlanDTO.position,
                Stime = mediaPlanDTO.stime,
                Etime = mediaPlanDTO.etime,
                Blocktime = mediaPlanDTO.blocktime,
                Days = mediaPlanDTO.days,
                Type = mediaPlanDTO.type,
                Special = mediaPlanDTO.special,
                Sdate = mediaPlanDTO.sdate.ToString("yyyy-MM-dd"),
                Edate = mediaPlanDTO.edate?.ToString("yyyy-MM-dd"),
                Progcoef = mediaPlanDTO.progcoef,
                Created = mediaPlanDTO.created.ToString("yyyy-MM-dd"),
                Modified = mediaPlanDTO.modified?.ToString("yyyy-MM-dd"),
                Amr1 = mediaPlanDTO.amr1,
                Amr1trim = mediaPlanDTO.amr1trim,
                Amr2 = mediaPlanDTO.amr2,
                Amr2trim = mediaPlanDTO.amr2trim,
                Amr3 = mediaPlanDTO.amr3,
                Amr3trim = mediaPlanDTO.amr3trim,
                Amrsale = mediaPlanDTO.amrsale,
                Amrsaletrim = mediaPlanDTO.amrsaletrim,
                Amrp1 = mediaPlanDTO.amrp1,
                Amrp2 = mediaPlanDTO.amrp2,
                Amrp3 = mediaPlanDTO.amrp3,
                Amrpsale = mediaPlanDTO.amrpsale,
                Dpcoef = mediaPlanDTO.dpcoef,
                Seascoef = mediaPlanDTO.seascoef,
                Price = mediaPlanDTO.price,
                Active = mediaPlanDTO.active
            });


            return affected != 0;
        }

        public async Task<MediaPlanDTO> GetMediaPlanById(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE id = @Id", new { Id = id });

            mediaPlan = mediaPlan.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = item.naziv,
                version = item.verzija,
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
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (double)item.amr1,
                amr1trim = (double)item.amr1trim,
                amr2 = (double)item.amr2,
                amr2trim = (double)item.amr2trim,
                amr3 = (double)item.amr3,
                amr3trim = (double)item.amr3trim,
                amrsale = (double)item.amrsale,
                amrsaletrim = (double)item.amrsaletrim,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                dpcoef = (double)item.dpkoef,
                seascoef = (double)item.seaskoef,
                price = (double)item.price,
                active = item.active
            });

            return _mapper.Map<MediaPlanDTO>(mediaPlan.FirstOrDefault());
        }

        public async Task<MediaPlanDTO> GetMediaPlanBySchemaId(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE schid = @Id", new { Id = id });
            
            mediaPlan = mediaPlan.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = item.naziv,
                version = item.verzija,
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
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (double)item.amr1,
                amr1trim = (double)item.amr1trim,
                amr2 = (double)item.amr2,
                amr2trim = (double)item.amr2trim,
                amr3 = (double)item.amr3,
                amr3trim = (double)item.amr3trim,
                amrsale = (double)item.amrsale,
                amrsaletrim = (double)item.amrsaletrim,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                dpcoef = (double)item.dpkoef,
                seascoef = (double)item.seaskoef,
                price = (double)item.price,
                active = item.active
            });

            return _mapper.Map<MediaPlanDTO>(mediaPlan.FirstOrDefault());
        }

        public async Task<MediaPlanDTO?> GetMediaPlanBySchemaAndCmpId(int schemaid, int cmpid)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE schid = @Schemaid AND cmpid = @Cmpid", 
                new { Schemaid = schemaid, Cmpid = cmpid });

            mediaPlan = mediaPlan.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = item.naziv,
                version = item.verzija,
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
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (double)item.amr1,
                amr1trim = (double)item.amr1trim,
                amr2 = (double)item.amr2,
                amr2trim = (double)item.amr2trim,
                amr3 = (double)item.amr3,
                amr3trim = (double)item.amr3trim,
                amrsale = (double)item.amrsale,
                amrsaletrim = (double)item.amrsaletrim,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                dpcoef = (double)item.dpkoef,
                seascoef = (double)item.seaskoef,
                price = (double)item.price,
                active = item.active
            });

            return _mapper.Map<MediaPlanDTO>(mediaPlan.FirstOrDefault());
        }

        public async Task<MediaPlanDTO> GetMediaPlanByCmpId(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE cmpid = @Cmpid", new { Cmpid = id });

            mediaPlan = mediaPlan.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = item.naziv,
                version = item.verzija,
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
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (double)item.amr1,
                amr1trim = (double)item.amr1trim,
                amr2 = (double)item.amr2,
                amr2trim = (double)item.amr2trim,
                amr3 = (double)item.amr3,
                amr3trim = (double)item.amr3trim,
                amrsale = (double)item.amrsale,
                amrsaletrim = (double)item.amrsaletrim,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                dpcoef = (double)item.dpkoef,
                seascoef = (double)item.seaskoef,
                price = (double)item.price,
                active = item.active
            });

            return _mapper.Map<MediaPlanDTO>(mediaPlan.FirstOrDefault());
        }

        public async Task<MediaPlanDTO> GetMediaPlanByName(string name)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE naziv = @Name", new { Name = name });

            mediaPlan = mediaPlan.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = item.naziv,
                version = item.verzija,
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
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (double)item.amr1,
                amr1trim = (double)item.amr1trim,
                amr2 = (double)item.amr2,
                amr2trim = (double)item.amr2trim,
                amr3 = (double)item.amr3,
                amr3trim = (double)item.amr3trim,
                amrsale = (double)item.amrsale,
                amrsaletrim = (double)item.amrsaletrim,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                dpcoef = (double)item.dpkoef,
                seascoef = (double)item.seaskoef,
                price = (double)item.price,
                active = item.active
            });

            return _mapper.Map<MediaPlanDTO>(mediaPlan.FirstOrDefault());
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlans()
        {
            using var connection = _context.GetConnection();

            var allMediaPlans = await connection.QueryAsync<dynamic>
                ("SELECT * FROM xmp");

            allMediaPlans = allMediaPlans.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = item.naziv,
                version = item.verzija,
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
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (double)item.amr1,
                amr1trim = (double)item.amr1trim,
                amr2 = (double)item.amr2,
                amr2trim = (double)item.amr2trim,
                amr3 = (double)item.amr3,
                amr3trim = (double)item.amr3trim,
                amrsale = (double)item.amrsale,
                amrsaletrim = (double)item.amrsaletrim,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                dpcoef = (double)item.dpkoef,
                seascoef = (double)item.seaskoef,
                price = (double)item.price,
                active = item.active
            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(allMediaPlans);
        }

        public async Task<IEnumerable<int>> GetAllChannelsByCmpid(int cmpid)
        {
            using var connection = _context.GetConnection();

            var allChannelIds = await connection.QueryAsync<int>
                ("SELECT chid FROM xmp WHERE cmpid = @Cmpid GROUP BY chid", 
                new { Cmpid = cmpid });

            return _mapper.Map<IEnumerable<int>>(allChannelIds);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansByCmpid(int cmpid)
        {
            using var connection = _context.GetConnection();

            var mediaPlans = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE cmpid = @Cmpid",
                new { Cmpid = cmpid });

            mediaPlans = mediaPlans.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = item.naziv,
                version = item.verzija,
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
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (double)item.amr1,
                amr1trim = (double)item.amr1trim,
                amr2 = (double)item.amr2,
                amr2trim = (double)item.amr2trim,
                amr3 = (double)item.amr3,
                amr3trim = (double)item.amr3trim,
                amrsale = (double)item.amrsale,
                amrsaletrim = (double)item.amrsaletrim,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                dpcoef = (double)item.dpkoef,
                seascoef = (double)item.seaskoef,
                price = (double)item.price,
                active = item.active
            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(mediaPlans);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansWithinDate(DateOnly sdate, DateOnly edate)
        {
            using var connection = _context.GetConnection();

            var allMediaPlans = await connection.QueryAsync<dynamic>
                ("SELECT * FROM xmp WHERE datumod <= @Edate AND datumdo >= @Sdate "
                , new { Sdate=sdate, Edate=edate});

            allMediaPlans = allMediaPlans.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = item.naziv,
                version = item.verzija,
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
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (double)item.amr1,
                amr1trim = (double)item.amr1trim,
                amr2 = (double)item.amr2,
                amr2trim = (double)item.amr2trim,
                amr3 = (double)item.amr3,
                amr3trim = (double)item.amr3trim,
                amrsale = (double)item.amrsale,
                amrsaletrim = (double)item.amrsaletrim,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                dpcoef = (double)item.dpkoef,
                seascoef = (double)item.seaskoef,
                price = (double)item.price,
                active = item.active
            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(allMediaPlans);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllChannelMediaPlans(int chid)
        {
            using var connection = _context.GetConnection();

            var mediaPlans = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE chid = @Chid",
                new { Chid = chid });

            mediaPlans = mediaPlans.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = item.naziv,
                version = item.verzija,
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
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (double)item.amr1,
                amr1trim = (double)item.amr1trim,
                amr2 = (double)item.amr2,
                amr2trim = (double)item.amr2trim,
                amr3 = (double)item.amr3,
                amr3trim = (double)item.amr3trim,
                amrsale = (double)item.amrsale,
                amrsaletrim = (double)item.amrsaletrim,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                dpcoef = (double)item.dpkoef,
                seascoef = (double)item.seaskoef,
                price = (double)item.price,
                active = item.active
            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(mediaPlans);
        }

        public async Task<bool> UpdateMediaPlan(UpdateMediaPlanDTO mediaPlanDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE xmp SET xmpid = @Xmpid, schid = @Schid, cmpid = @Cmpid, chid = @Chid, naziv = @Name, " +
                "verzija = @Version, pozicija = @Position, " +
                "vremeod = @Stime, vremedo = @Etime, vremerbl = @Blocktime, dani = @Days, " +
                "tipologija = @Type, specijal = @Special, datumod = CAST(@Sdate AS DATE), datumdo = CAST(@Edate AS DATE), " +
                "datumkreiranja = CAST(@Created AS DATE), datumizmene = CAST(@Modified AS DATE), " +
                "amr1 = @Amr1, amr1trim = @Amr1trim, amr2 = @Amr2, amr2trim = @Amr2trim, amr3 = @Amr3, amr3trim = @Amr3trim, amrsale = @Amrsale, amrsaletrim = @Amrsaletrim, " +
                "amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, amrpsale = @Amrpsale, " +
                "dpkoef = @Dpcoef, seaskoef = @Seascoef, price = @Price, active = @Active " +
                "WHERE xmpid = @Xmpid",
                new
                {
                    Xmpid = mediaPlanDTO.xmpid,
                    Schid = mediaPlanDTO.schid,
                    Chid = mediaPlanDTO.chid,
                    Name = mediaPlanDTO.name,
                    Version = mediaPlanDTO.version,
                    Position = mediaPlanDTO.position,
                    Stime = mediaPlanDTO.stime,
                    Etime = mediaPlanDTO.etime,
                    Blocktime = mediaPlanDTO.blocktime,
                    Days = mediaPlanDTO.days,
                    Type = mediaPlanDTO.type,
                    Special = mediaPlanDTO.special,
                    Sdate = mediaPlanDTO.sdate.ToString("yyyy-MM-dd"),
                    Edate = mediaPlanDTO.edate == null ? null : mediaPlanDTO.edate.Value.ToString("yyyy-MM-dd"),
                    Progcoef = mediaPlanDTO.progcoef,
                    Created = mediaPlanDTO.created.ToString("yyyy-MM-dd"),
                    Modified = mediaPlanDTO.modified == null ? null : mediaPlanDTO.modified.Value.ToString("yyyy-MM-dd"),
                    Amr1 = mediaPlanDTO.amr1,
                    Amr1trim = mediaPlanDTO.amr1trim,
                    Amr2 = mediaPlanDTO.amr2,
                    Amr2trim = mediaPlanDTO.amr2trim,
                    Amr3 = mediaPlanDTO.amr3,
                    Amr3trim = mediaPlanDTO.amr3trim,
                    Amrsale = mediaPlanDTO.amrsale,
                    Amrsaletrim = mediaPlanDTO.amrsaletrim,
                    Amrp1 = mediaPlanDTO.amrp1,
                    Amrp2 = mediaPlanDTO.amrp2,
                    Amrp3 = mediaPlanDTO.amrp3,
                    Amrpsale = mediaPlanDTO.amrpsale,
                    Dpcoef = mediaPlanDTO.dpcoef,
                    Seascoef = mediaPlanDTO.seascoef,
                    Price = mediaPlanDTO.price,
                    Active = mediaPlanDTO.active
                });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM xmp WHERE xmpid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanByCmpId(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM xmp WHERE cmpid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> SetActiveMediaPlanById(int id, bool isActive)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE xmp SET active = @Active " +
                "WHERE xmpid = @Id", new { Active = isActive, Id = id });

            return affected != 0;
        }

    }
}
