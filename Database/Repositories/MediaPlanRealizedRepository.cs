using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanRealizedDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class MediaPlanRealizedRepository : IMediaPlanRealizedRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public MediaPlanRealizedRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateMediaPlanRealized(CreateMediaPlanRealizedDTO mediaPlanRealizedDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO xmpre (naziv, cmpid, vremeod, vremedo, vremeodv, vremedov, chid, dure, durf, datum, bremisije  " +
                " pozinbr, totspotbr, breaktype, brspot, brbrand, amrp1, amrp2, amrp3, amrpsale " +
                " cpp, dpkoef, seaskoef, seckoef, progkoef, cena, status, chcoef, koefa, koefb, accept " + 
                "VALUES (@Name, @Cmpid, @Stime, @Etime, @Stimestr, @Etimestr, @Chid, @Dure, @Durf, @Date, @Emsnum, " +
                " @Posinbr, @Totalspotnum, @Breaktype, @Spotnum, @Brandnum, @Amrp1, @Amrp2, @Amrp3, @Amrpsale " +
                "@Cpp, @Dpcoef, @Seascoef, @Progcoef, @Price, @Status, @Chcoef, @CoefA, @CoefB, @Accept ) ",
            new
            {
                Name = mediaPlanRealizedDTO.name.Trim(),
                Cmpid = mediaPlanRealizedDTO.cmpid,
                Stime = mediaPlanRealizedDTO.stime,
                Etime = mediaPlanRealizedDTO.etime,
                Stimestr = mediaPlanRealizedDTO.stimestr,
                Etimestr = mediaPlanRealizedDTO.etimestr,
                Chid = mediaPlanRealizedDTO.chid,
                Dure = mediaPlanRealizedDTO.dure,
                Durf = mediaPlanRealizedDTO.durf,
                Date = mediaPlanRealizedDTO.date,
                Emsnum = mediaPlanRealizedDTO.emsnum,
                Posinbr = mediaPlanRealizedDTO.posinbr,
                Totalspotnum = mediaPlanRealizedDTO.totalspotnum,
                Breaktype = mediaPlanRealizedDTO.breaktype,
                Spotnum = mediaPlanRealizedDTO.spotnum,
                Brandnum = mediaPlanRealizedDTO.brandnum,
                Amrp1 = mediaPlanRealizedDTO.amrp1,
                Amrp2 = mediaPlanRealizedDTO.amrp2,
                Amrp3 = mediaPlanRealizedDTO.amrp3,
                Amrpsale = mediaPlanRealizedDTO.amrpsale,
                Cpp = mediaPlanRealizedDTO.cpp,
                Dpcoef = mediaPlanRealizedDTO.dpcoef,
                Seascoef = mediaPlanRealizedDTO.seascoef,
                Seccoef = mediaPlanRealizedDTO.seccoef,
                Progcoef = mediaPlanRealizedDTO.progcoef,
                Price = mediaPlanRealizedDTO.price,
                Status = mediaPlanRealizedDTO.status,
                Chcoef = mediaPlanRealizedDTO.chcoef,
                CoefA = mediaPlanRealizedDTO.coefA,
                CoefB = mediaPlanRealizedDTO.coefB,
                Accept = mediaPlanRealizedDTO.accept
            });


            return affected != 0;
        }

        public async Task<MediaPlanRealizedDTO> GetMediaPlanRealizedById(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlanRealized = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmpre WHERE id = @Id", new { Id = id });

            mediaPlanRealized = mediaPlanRealized.Select(item => new MediaPlanRealized()
            {
                id = item.id,
                cmpid = item.cmpid,
                name = item.naziv.Trim(),
                stime = item.vremeod,
                etime = item.vremedo,
                stimestr = item.vremeodv,
                etimestr = item.vremedov,
                chid = item.chid,
                dure = item.dure,
                durf = item.durf,
                date = item.datum,
                emsnum = item.bremisije,
                posinbr = item.pozinbr,
                totalspotnum = item.totspotbr,
                breaktype = item.breaktype,
                spotnum = item.brspot,
                brandnum = item.brbrand,
                amrp1 = item.amrp1,
                amrp2 = item.amrp2,
                amrp3 = item.amrp3,
                amrpsale = item.amrpsale,
                Cpp = item.cpp ?? null,
                Dpcoef = item.dpkoef ?? null,
                Seascoef = item.seaskoef ?? null,
                Seccoef = item.seckoef ?? null,
                Progcoef = item.progkoef ?? null,
                price = item.cena ?? null,
                status = item.status ?? null,
                Chcoef = item.chcoef ?? null,
                CoefA = item.koefa ?? null,
                CoefB = item.koefb ?? null,
                Accept = item.accept ?? false
            });

            return _mapper.Map<MediaPlanRealizedDTO>(mediaPlanRealized.FirstOrDefault());
        }

        public async Task<IEnumerable<MediaPlanRealized>> GetAllMediaPlansRealizedByCmpid(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlanRealized = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmpre WHERE cmpid = @Id", new { Id = id });

            mediaPlanRealized = mediaPlanRealized.Select(item => new MediaPlanRealized()
            {
                id = item.id,
                cmpid = Convert.ToInt32(item.cmpid),
                name = item.naziv.Trim(),
                stime = item.vremeod,
                etime = item.vremedo,
                stimestr = item.vremeodv,
                etimestr = item.vremedov,
                chid = Convert.ToInt32(item.chid),
                dure = Convert.ToInt32(item.dure),
                durf = Convert.ToInt32(item.durf),
                date = item.datum,
                emsnum = Convert.ToInt32(item.bremisije),
                posinbr = item.pozinbr,
                totalspotnum = item.totspotbr,
                breaktype = item.breaktype,
                spotnum = Convert.ToInt32(item.brspot),
                brandnum = Convert.ToInt32(item.brbrand),
                amrp1 = item.amrp1,
                amrp2 = item.amrp2,
                amrp3 = item.amrp3,
                amrpsale = item.amrpsale,
                Cpp = item.cpp ?? null,
                Dpcoef = item.dpkoef ?? null,
                Seascoef = item.seaskoef ?? null,
                Seccoef = item.seckoef ?? null,
                Progcoef = item.progkoef ?? null,
                price = item.cena ?? null,
                status = item.status ?? null,
                Chcoef = item.chcoef ?? null,
                CoefA = item.koefa ?? null,
                CoefB = item.koefb ?? null,
                Accept = item.accept ?? false
            });

            return (IEnumerable<MediaPlanRealized>)mediaPlanRealized;
        }

        public async Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChid(int chid)
        {
            using var connection = _context.GetConnection();

            var mediaPlansRealized = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmpre WHERE chid = @Chid",
                new { Chid = chid });

            mediaPlansRealized = mediaPlansRealized.Select(item => new MediaPlanRealized()
            {
                id = item.id,
                cmpid = item.cmpid,
                name = item.naziv.Trim(),
                stime = item.vremeod,
                etime = item.vremedo,
                stimestr = item.vremeodv,
                etimestr = item.vremedov,
                chid = item.chid,
                dure = item.dure,
                durf = item.durf,
                date = item.datum,
                emsnum = item.bremisije,
                posinbr = item.pozinbr,
                totalspotnum = item.totspotbr,
                breaktype = item.breaktype,
                spotnum = item.brspot,
                brandnum = item.brbrand,
                amrp1 = item.amrp1,
                amrp2 = item.amrp2,
                amrp3 = item.amrp3,
                amrpsale = item.amrpsale,
                Cpp = item.cpp ?? null,
                Dpcoef = item.dpkoef ?? null,
                Seascoef = item.seaskoef ?? null,
                Seccoef = item.seckoef ?? null,
                Progcoef = item.progkoef ?? null,
                price = item.cena ?? null,
                status = item.status ?? null,
                Chcoef = item.chcoef ?? null,
                CoefA = item.koefa ?? null,
                CoefB = item.koefb ?? null,
                Accept = item.accept ?? false
            });

            return _mapper.Map<IEnumerable<MediaPlanRealizedDTO>>(mediaPlansRealized);

        }

        public async Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChidAndDate(int chid, string date)
        {
            using var connection = _context.GetConnection();

            var mediaPlansRealized = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmpre WHERE chid = @Chid AND datum = @Date",
                new { Chid = chid, Date = date });

            mediaPlansRealized = mediaPlansRealized.Select(item => new MediaPlanRealized()
            {
                id = item.id,
                cmpid = item.cmpid,
                name = item.naziv.Trim(),
                stime = item.vremeod,
                etime = item.vremedo,
                stimestr = item.vremeodv,
                etimestr = item.vremedov,
                chid = item.chid,
                dure = item.dure,
                durf = item.durf,
                date = item.datum,
                emsnum = item.bremisije,
                posinbr = item.pozinbr,
                totalspotnum = item.totspotbr,
                breaktype = item.breaktype,
                spotnum = item.brspot,
                brandnum = item.brbrand,
                amrp1 = item.amrp1,
                amrp2 = item.amrp2,
                amrp3 = item.amrp3,
                amrpsale = item.amrpsale,
                Cpp = item.cpp ?? null,
                Dpcoef = item.dpkoef ?? null,
                Seascoef = item.seaskoef ?? null,
                Seccoef = item.seckoef ?? null,
                Progcoef = item.progkoef ?? null,
                price = item.cena ?? null,
                status = item.status ?? null,
                Chcoef = item.chcoef ?? null,
                CoefA = item.koefa ?? null,
                CoefB = item.koefb ?? null,
                Accept = item.accept ?? false
            });

            return _mapper.Map<IEnumerable<MediaPlanRealizedDTO>>(mediaPlansRealized);
        }

        public async Task<bool> UpdateMediaPlanRealized(UpdateMediaPlanRealizedDTO mediaPlanRealizedDTO)
        {
            using var connection = _context.GetConnection();


            var affected = await connection.ExecuteAsync(
                "UPDATE xmpre SET id = @Id, cmpid = @Cmpid, naziv = @Name, vremeod = @Stime, vremedo = @Etime, " +
                " vremeodv = @Stimestr, vremedov = @Etimestr, chid = @Chid, dure = @Dure, durf = @Durf, datum = @Date, bremisije = @Emsnum,  " +
                " pozinbr = @Posinbr, totspotbr = @Totalspotnum, breaktype = @Breaktype, " +
                " brspot = @Spotnum, brbrand = @Brandnum, amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, " +
                " amrpsale = @Amrpsale, cpp = @Cpp, dpkoef = @Dpcoef, seaskoef = @Seascoef, seckoef = @Seccoef" +
                " progkoef = @Progcoef, cena = @Price, status = @Status, chcoef = @Chcoef, koefa = @CoefA, koefb = @CoefB, accept = @Accept " +
                " WHERE id = @Id",
                new
                {
                    Id = mediaPlanRealizedDTO.id,
                    Name = mediaPlanRealizedDTO.name.Trim(),
                    Cmpid = mediaPlanRealizedDTO.cmpid,
                    Stime = mediaPlanRealizedDTO.stime,
                    Etime = mediaPlanRealizedDTO.etime,
                    Stimestr = mediaPlanRealizedDTO.stimestr,
                    Etimestr = mediaPlanRealizedDTO.etimestr,
                    Chid = mediaPlanRealizedDTO.chid,
                    Dure = mediaPlanRealizedDTO.dure,
                    Durf = mediaPlanRealizedDTO.durf,
                    Date = mediaPlanRealizedDTO.date,
                    Emsnum = mediaPlanRealizedDTO.emsnum,
                    Posinbr = mediaPlanRealizedDTO.posinbr,
                    Totalspotnum = mediaPlanRealizedDTO.totalspotnum,
                    Breaktype = mediaPlanRealizedDTO.breaktype,
                    Spotnum = mediaPlanRealizedDTO.spotnum,
                    Brandnum = mediaPlanRealizedDTO.brandnum,
                    Amrp1 = mediaPlanRealizedDTO.amrp1,
                    Amrp2 = mediaPlanRealizedDTO.amrp2,
                    Amrp3 = mediaPlanRealizedDTO.amrp3,
                    Amrpsale = mediaPlanRealizedDTO.amrpsale,
                    Cpp = mediaPlanRealizedDTO.cpp,
                    Dpcoef = mediaPlanRealizedDTO.dpcoef,
                    Seascoef = mediaPlanRealizedDTO.seascoef,
                    Seccoef = mediaPlanRealizedDTO.seccoef,
                    Progcoef = mediaPlanRealizedDTO.progcoef,
                    Price = mediaPlanRealizedDTO.price,
                    Status = mediaPlanRealizedDTO.status,
                    Chcoef = mediaPlanRealizedDTO.chcoef,
                    CoefA = mediaPlanRealizedDTO.coefA,
                    CoefB = mediaPlanRealizedDTO.coefB,
                    Accept = mediaPlanRealizedDTO.accept
                });



            return affected != 0;
        }

        public async Task<bool> UpdateMediaPlanRealized(MediaPlanRealized mediaPlanRealized)
        {
            using var connection = _context.GetConnection();


            var affected = await connection.ExecuteAsync(
                "UPDATE xmpre SET id = @Id, cmpid = @Cmpid, naziv = @Name, vremeod = @Stime, vremedo = @Etime, " +
                " vremeodv = @Stimestr, vremedov = @Etimestr, chid = @Chid, dure = @Dure, durf = @Durf, datum = @Date, bremisije = @Emsnum, " +
                " pozinbr = @Posinbr, totspotbr = @Totalspotnum, breaktype = @Breaktype, " +
                " brspot = @Spotnum, brbrand = @Brandnum, amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, amrpsale = @Amrpsale, " +
                " cpp = @Cpp, dpkoef = @Dpcoef, seaskoef = @Seascoef, seckoef = @Seccoef, " +
                " progkoef = @Progcoef, cena = @Price, status = @Status, chcoef = @Chcoef, koefa = @CoefA, koefb = @CoefB, accept = @Accept " +
                " WHERE id = @Id",
                new
                {
                    Id = mediaPlanRealized.id.GetValueOrDefault(),
                    Name = mediaPlanRealized.name?.Trim(),
                    Cmpid = mediaPlanRealized.cmpid.GetValueOrDefault(),
                    Stime = mediaPlanRealized.stime.GetValueOrDefault(),
                    Etime = mediaPlanRealized.etime.GetValueOrDefault(),
                    Stimestr = mediaPlanRealized.stimestr,
                    Etimestr = mediaPlanRealized.etimestr,
                    Chid = mediaPlanRealized.chid.GetValueOrDefault(),
                    Dure = mediaPlanRealized.dure.GetValueOrDefault(),
                    Durf = mediaPlanRealized.durf.GetValueOrDefault(),
                    Date = mediaPlanRealized.date,
                    Emsnum = mediaPlanRealized.emsnum.GetValueOrDefault(),
                    Posinbr = mediaPlanRealized.posinbr.GetValueOrDefault(),
                    Totalspotnum = mediaPlanRealized.totalspotnum.GetValueOrDefault(),
                    Breaktype = mediaPlanRealized.breaktype.GetValueOrDefault(),
                    Spotnum = mediaPlanRealized.spotnum.GetValueOrDefault(),
                    Brandnum = mediaPlanRealized.brandnum.GetValueOrDefault(),
                    Amrp1 = mediaPlanRealized.amrp1,
                    Amrp2 = mediaPlanRealized.amrp2,
                    Amrp3 = mediaPlanRealized.amrp3,
                    Amrpsale = mediaPlanRealized.amrpsale,
                    Cpp = mediaPlanRealized.Cpp.GetValueOrDefault(),
                    Dpcoef = mediaPlanRealized.Dpcoef.GetValueOrDefault(),
                    Seascoef = mediaPlanRealized.Seascoef.GetValueOrDefault(),
                    Seccoef = mediaPlanRealized.Seccoef.GetValueOrDefault(),
                    Progcoef = mediaPlanRealized.Progcoef.GetValueOrDefault(),
                    Price = mediaPlanRealized.price.GetValueOrDefault(),
                    Status = mediaPlanRealized.status.GetValueOrDefault(),
                    Chcoef = mediaPlanRealized.Chcoef.GetValueOrDefault(),
                    CoefA = mediaPlanRealized.CoefA.GetValueOrDefault(),
                    CoefB = mediaPlanRealized.CoefB.GetValueOrDefault(),
                    Accept = mediaPlanRealized.Accept.GetValueOrDefault()
                });



            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanRealizedById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM xmpre WHERE id = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<string> GetDedicatedSpotName(int spotid)
        {
            using var connection = _context.GetConnection();

            string nazreklame = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT nazreklame FROM spotovi WHERE brreklame = @Id", new { Id = spotid });

            return nazreklame;
        }

        public async Task<bool> SetStatusValue(int id, int statusValue)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE xmpre SET status = @StatusValue WHERE id = @Id", 
                new { Id = id, StatusValue = statusValue });

            return affected != 0;
        }
    }
}
