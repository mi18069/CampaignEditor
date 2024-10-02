using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanDTO;
using Database.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
                "amr1, amr1trim, amr2, amr2trim, amr3, amr3trim, amrsale, amrsaletrim, amrp1, amrp2, amrp3, amrpsale, dpkoef, seaskoef, seckoef, price, active, pricepersec, koefa, koefb, cbrkoef) " +
                "VALUES (@Schid, @Cmpid, @Chid, @Name, @Version, @Position, @Stime, @Etime, @Blocktime, " +
                "@Days, @Type, @Special, CAST (@Sdate AS DATE), CAST(@Edate AS DATE), @Progcoef, CAST(@Created AS DATE), CAST(@Modified AS DATE), " +
                "@Amr1, @Amr1trim, @Amr2, @Amr2trim, @Amr3, @Amr3trim, @Amrsale, @Amrsaletrim, @Amrp1, @Amrp2, @Amrp3, @Amrpsale, @Dpcoef, @Seascoef, @Seccoef, @Price, @Active, @PricePerSec, @CoefA, @CoefB, @Cbrcoef) ",
            new
            {
                Schid = mediaPlanDTO.schid,
                Cmpid = mediaPlanDTO.cmpid,
                Chid = mediaPlanDTO.chid,
                Name = mediaPlanDTO.name.Trim(),
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
                Seccoef = mediaPlanDTO.seccoef,
                Price = mediaPlanDTO.price,
                Active = mediaPlanDTO.active,
                PricePerSec = Math.Round(mediaPlanDTO.pps, 2),
                CoefA = mediaPlanDTO.coefA,
                CoefB = mediaPlanDTO.coefB,
                Cbrcoef = mediaPlanDTO.cbrcoef
            });


            return affected != 0;
        }

        public async Task<MediaPlanDTO> CreateAndReturnMediaPlan(CreateMediaPlanDTO mediaPlanDTO)
        {
            using var connection = _context.GetConnection();

            var result = await connection.QuerySingleAsync<MediaPlanDTO>(
                "INSERT INTO xmp (schid, cmpid, chid, naziv, verzija, pozicija, vremeod, vremedo, vremerbl, " +
                "dani, tipologija, specijal, datumod, datumdo, progkoef, datumkreiranja, datumizmene, " +
                "amr1, amr1trim, amr2, amr2trim, amr3, amr3trim, amrsale, amrsaletrim, amrp1, amrp2, amrp3, amrpsale, dpkoef, seaskoef, seckoef, price, active, pricepersec, koefa, koefb, cbrkoef) " +
                "OUTPUT INSERTED.Id " + // Retrieve the newly inserted Id
                "VALUES (@Schid, @Cmpid, @Chid, @Name, @Version, @Position, @Stime, @Etime, @Blocktime, " +
                "@Days, @Type, @Special, CAST (@Sdate AS DATE), CAST(@Edate AS DATE), @Progcoef, CAST(@Created AS DATE), CAST(@Modified AS DATE), " +
                "@Amr1, @Amr1trim, @Amr2, @Amr2trim, @Amr3, @Amr3trim, @Amrsale, @Amrsaletrim, @Amrp1, @Amrp2, @Amrp3, @Amrpsale, @Dpcoef, @Seascoef, @Seccoef, @Price, @Active, @PricePerSec, @CoefA, @CoefB, @Cbrcoef) ",
            new
            {
                Schid = mediaPlanDTO.schid,
                Cmpid = mediaPlanDTO.cmpid,
                Chid = mediaPlanDTO.chid,
                Name = mediaPlanDTO.name.Trim(),
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
                Seccoef = mediaPlanDTO.seccoef,
                Price = mediaPlanDTO.price,
                Active = mediaPlanDTO.active,
                PricePerSec = Math.Round(mediaPlanDTO.pps, 2),
                CoefA = mediaPlanDTO.coefA,
                CoefB = mediaPlanDTO.coefB,
                Cbrcoef = mediaPlanDTO.cbrcoef
            });

            if (result != null)
            {
                // Successfully inserted, 'result' contains the newly inserted MediaPlan with Id
                return result;
            }
            else
            {
                // Insertion failed, handle accordingly (e.g., throw an exception)
                return null;
            }
        }

        public async Task<MediaPlanDTO> GetMediaPlanById(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE xmpid = @Id", new { Id = id });

            mediaPlan = mediaPlan.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef
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
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef
            });

            return _mapper.Map<MediaPlanDTO>(mediaPlan.FirstOrDefault());
        }

        public async Task<MediaPlanDTO?> GetMediaPlanBySchemaAndCmpId(int schemaid, int cmpid, int version)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE schid = @Schemaid AND cmpid = @Cmpid AND verzija = @Version", 
                new { Schemaid = schemaid, Cmpid = cmpid, Version = version });

            mediaPlan = mediaPlan.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef

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
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef

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
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef

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
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef

            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(allMediaPlans);
        }

        public async Task<IEnumerable<int>> GetAllChannelsByCmpid(int cmpid, int version)
        {
            using var connection = _context.GetConnection();

            var allChannelIds = await connection.QueryAsync<int>
                ("SELECT chid FROM xmp WHERE cmpid = @Cmpid AND verzija = @Version GROUP BY chid", 
                new { Cmpid = cmpid, Version = version });

            return _mapper.Map<IEnumerable<int>>(allChannelIds);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansByCmpid(int cmpid, int version)
        {
            using var connection = _context.GetConnection();

            var mediaPlans = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE cmpid = @Cmpid AND verzija = @Version",
                new { Cmpid = cmpid, Version = version });

            mediaPlans = mediaPlans.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef

            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(mediaPlans);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansByCmpidAndChannel(int cmpid, int chid, int version)
        {
            using var connection = _context.GetConnection();

            var mediaPlanList= await connection.QueryAsync<MediaPlanDBEntity>(
                "SELECT * FROM xmp WHERE cmpid = @Cmpid AND chid = @Chid AND verzija = @Version",
                new { Cmpid = cmpid, Chid = chid, Version = version });

            /*
            var mediaPlans = mediaPlanList.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = (string)item.naziv.Trim(),
                version = item.verzija,
                position = item.pozicija,
                stime = item.vremeod,
                etime = item.vremedo == null ? null : item.vremedo,
                blocktime = item.vremerbl == null ? null : item.vremerbl,
                days = item.dani,
                type = item.tipologija,
                special = item.specijal,
                sdate = DateOnly.FromDateTime(item.datumod),
                edate = item.datumdo.HasValue ? DateOnly.FromDateTime(item.datumdo.Value) : (DateOnly?)null,
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene.HasValue ? DateOnly.FromDateTime(item.datumizmene.Value) : (DateOnly?)null,
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pricepersec != null ? (decimal)item.pricepersec : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb
            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(mediaPlans);*/
            var mediaPlans = mediaPlanList.Select(item => new MediaPlanDTO(
                item.xmpid,
                item.schid,
                item.cmpid,
                item.chid,
                (string)item.naziv.Trim(),
                item.verzija,
                item.pozicija,
                item.vremeod,
                item.vremedo == null ? null : item.vremedo,
                item.vremerbl == null ? null : item.vremerbl,
                item.dani,
                item.tipologija,
                item.specijal,
                DateOnly.FromDateTime(item.datumod),
                item.datumdo.HasValue ? DateOnly.FromDateTime(item.datumdo.Value) : (DateOnly?)null,
                (decimal)item.progkoef,
                DateOnly.FromDateTime(item.datumkreiranja),
                item.datumizmene.HasValue ? DateOnly.FromDateTime(item.datumizmene.Value) : (DateOnly?)null,
                (decimal)item.amr1,
                item.amr1trim,
                (decimal)item.amr2,
                item.amr2trim,
                (decimal)item.amr3,
                item.amr3trim,
                (decimal)item.amrsale,
                item.amrsaletrim,
                (decimal)item.amrp1,
                (decimal)item.amrp2,
                (decimal)item.amrp3,
                (decimal)item.amrpsale,
                (decimal)item.dpkoef,
                (decimal)item.seaskoef,
                (decimal)item.seckoef,
                (decimal)item.koefa,
                (decimal)item.koefb,
                (decimal)item.cbrkoef,
                (decimal)item.price,
                item.active,
                item.pricepersec != null ? (decimal)item.pricepersec : 0.0M
                ));

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(mediaPlans);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansByCmpidAndChannelAllVersions(int cmpid, int chid)
        {
            using var connection = _context.GetConnection();

            var mediaPlans = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE cmpid = @Cmpid AND chid = @Chid ",
                new { Cmpid = cmpid, Chid = chid});

            mediaPlans = mediaPlans.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef

            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(mediaPlans);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansWithinDate(DateOnly sdate, DateOnly edate)
        {
            using var connection = _context.GetConnection();

            var allMediaPlans = await connection.QueryAsync<dynamic>
                ("SELECT * FROM xmp WHERE datumod <= @Edate AND datumdo >= @Sdate"
                , new { Sdate=sdate, Edate=edate});

            allMediaPlans = allMediaPlans.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef

            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(allMediaPlans);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllChannelCmpMediaPlans(int chid, int cmpid, int version)
        {
            using var connection = _context.GetConnection();

            var mediaPlans = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmp WHERE chid = @Chid AND cmpid = @Cmpid AND verzija = @Version",
                new { Chid = chid, Cmpid = cmpid, Version = version });

            mediaPlans = mediaPlans.Select(item => new MediaPlan()
            {
                xmpid = item.xmpid,
                schid = item.schid,
                cmpid = item.cmpid,
                chid = item.chid,
                name = (string)item.naziv.Trim(),
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
                progcoef = (decimal)item.progkoef,
                created = DateOnly.FromDateTime(item.datumkreiranja),
                modified = item.datumizmene == null ? null : DateOnly.FromDateTime(item.datumizmene),
                amr1 = (decimal)item.amr1,
                amr1trim = item.amr1trim,
                amr2 = (decimal)item.amr2,
                amr2trim = item.amr2trim,
                amr3 = (decimal)item.amr3,
                amr3trim = item.amr3trim,
                amrsale = (decimal)item.amrsale,
                amrsaletrim = item.amrsaletrim,
                amrp1 = (decimal)item.amrp1,
                amrp2 = (decimal)item.amrp2,
                amrp3 = (decimal)item.amrp3,
                amrpsale = (decimal)item.amrpsale,
                dpcoef = (decimal)item.dpkoef,
                seascoef = (decimal)item.seaskoef,
                seccoef = (decimal)item.seckoef,
                price = (decimal)item.price,
                active = item.active,
                pps = item.pps != null ? (decimal)item.pps : 0.0M,
                coefA = (decimal)item.koefa,
                coefB = (decimal)item.koefb,
                cbrcoef = (decimal)item.cbrkoef

            });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(mediaPlans);
        }

        public async Task<bool> UpdateMediaPlan(UpdateMediaPlanDTO mediaPlanDTO)
        {
            using var connection = _context.GetConnection();
            
            
            var affected = await connection.ExecuteAsync(
                @"
                UPDATE xmp SET 
                    xmpid = @Xmpid, schid = @Schid, cmpid = @Cmpid, chid = @Chid, 
                    naziv = @Name, verzija = @Version, pozicija = @Position, vremeod = @Stime, 
                    vremedo = @Etime, vremerbl = @Blocktime, dani = @Days, tipologija = @Type, 
                    specijal = @Special, datumod = CAST(@Sdate AS DATE), datumdo = CAST(@Edate AS DATE), 
                    datumkreiranja = CAST(@Created AS DATE), datumizmene = CAST(@Modified AS DATE), 
                    amr1 = @Amr1, amr1trim = @Amr1trim, amr2 = @Amr2, amr2trim = @Amr2trim, 
                    amr3 = @Amr3, amr3trim = @Amr3trim, amrsale = @Amrsale, amrsaletrim = @Amrsaletrim, 
                    amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, amrpsale = @Amrpsale, 
                    dpkoef = @Dpcoef, progkoef = @Progcoef, seaskoef = @Seascoef, seckoef = @Seccoef, 
                    price = @Price, active = @Active, pricepersec = @PricePerSec, 
                    koefa = @CoefA, koefb = @CoefB, cbrkoef = @Cbrcoef 
                WHERE xmpid = @Xmpid",
                new
                {
                    Xmpid = mediaPlanDTO.xmpid,
                    Schid = mediaPlanDTO.schid,
                    Cmpid = mediaPlanDTO.cmpid,
                    Chid = mediaPlanDTO.chid,
                    Name = mediaPlanDTO.name.Trim(),
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
                    Seccoef = mediaPlanDTO.seccoef,
                    Price = mediaPlanDTO.price,
                    Active = mediaPlanDTO.active,
                    PricePerSec = Math.Round(mediaPlanDTO.pps, 2),
                    CoefA = mediaPlanDTO.coefA,
                    CoefB = mediaPlanDTO.coefB,
                    Cbrcoef = mediaPlanDTO.cbrcoef
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

        public async Task<List<int>> DuplicateMediaPlans(IEnumerable<MediaPlan> mediaPlans, int newVersion)
        {
            using var connection = _context.GetConnection();

            StringBuilder sbQuery = new StringBuilder();
            int batchSize = 100; 
            int currentBatch = 0;
            bool firstMp = true;
            List<int> mediaPlanIdsList = new List<int>();

            foreach (var mediaPlan in mediaPlans)
            {
                if (currentBatch == 0)
                {
                    sbQuery.Append("INSERT INTO xmp (schid, cmpid, chid, naziv, verzija, pozicija, vremeod, vremedo, vremerbl, dani, tipologija, specijal, datumod, datumdo, progkoef, datumkreiranja, datumizmene, amr1, amr1trim, amr2, amr2trim, amr3, amr3trim, amrsale, amrsaletrim, amrp1, amrp2, amrp3, amrpsale, dpkoef, seaskoef, seckoef, price, active, pricepersec, koefa, koefb, cbrkoef) VALUES ");
                    firstMp = true;
                }

                var values = $",({mediaPlan.schid}, {mediaPlan.cmpid}, {mediaPlan.chid}, '{mediaPlan.Name}', {newVersion}, '{mediaPlan.position}', '{mediaPlan.stime}', '{mediaPlan.etime}', {(mediaPlan.blocktime == null ? "NULL" : $"'{mediaPlan.blocktime}'")}, '{mediaPlan.days}', '{mediaPlan.type}', {mediaPlan.special}, CAST ('{mediaPlan.sdate.ToString("yyyy-MM-dd")}' AS DATE), {(mediaPlan.edate == null ? "NULL" : $"CAST('{mediaPlan.edate.Value.ToString("yyyy-MM-dd")}' AS DATE)")},  {mediaPlan.Progcoef}, CAST('{mediaPlan.created.ToString("yyyy-MM-dd")}' AS DATE), {(mediaPlan.modified == null ? "NULL" : $"CAST('{mediaPlan.modified.Value.ToString("yyyy-MM-dd")}' AS DATE)")}, {mediaPlan.Amr1}, {mediaPlan.Amr1trim}, {mediaPlan.Amr2}, {mediaPlan.Amr2trim}, {mediaPlan.Amr3}, {mediaPlan.amr3trim}, {mediaPlan.Amrsale}, {mediaPlan.Amrsaletrim}, {mediaPlan.Amrp1}, {mediaPlan.Amrp2}, {mediaPlan.Amrp3}, {mediaPlan.Amrpsale}, {mediaPlan.Dpcoef}, {mediaPlan.Seascoef}, {mediaPlan.Seccoef}, {mediaPlan.Price}, {mediaPlan.active}, {mediaPlan.PricePerSecond}, {mediaPlan.CoefA}, {mediaPlan.CoefB}, {mediaPlan.Cbrcoef})";

                if (!firstMp)
                {
                    sbQuery.Append(values);
                }
                else
                {
                    sbQuery.Append(values.Substring(1));
                    firstMp = false;
                }

                currentBatch++;

                if (currentBatch >= batchSize)
                {
                    // Add the RETURNING clause to fetch new IDs
                    sbQuery.Append(" RETURNING xmpid;");

                    var mediaPlanIds = await connection.QueryAsync<int>(sbQuery.ToString());
                    mediaPlanIdsList.AddRange(mediaPlanIds);

                    sbQuery.Clear(); // Reset for the next batch
                    currentBatch = 0;
                }
            }

            // If any remaining rows after the loop
            if (currentBatch > 0)
            {                    
                // Add the RETURNING clause to fetch new IDs
                sbQuery.Append(" RETURNING xmpid;");
                var mediaPlanIds = await connection.QueryAsync<int>(sbQuery.ToString());
                mediaPlanIdsList.AddRange(mediaPlanIds);
            }

            return mediaPlanIdsList;
        }
    }
}
