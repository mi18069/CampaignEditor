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
                " pozinbr, totalspotbr, breaktype, brspot, brbrand, amrp1, amrp2, amrp3, amrpsale " +
                " cpp, dpcoef, seascoef, progcoef, cena, status, chcoef, koefa, koefb " + 
                "VALUES (@Name, @Cmpid, @Stime, @Etime, @Stimestr, @Etimestr, @Chid, @Dure, @Durf, @Date, @Emsnum, " +
                " @Posinbr, @Totalspotnum, @Breaktype, @Spotnum, @Brandnum, @Amrp1, @Amrp2, @Amrp3, @Amrpsale " +
                "@Cpp, @Dpcoef, @Seascoef, @Progcoef, @Price, @Status, @Chcoef, @CoefA, @CoefB ) ",
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
                CoefB = mediaPlanRealizedDTO.coefB
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
                totalspotnum = item.totalspotbr,
                breaktype = item.breaktype,
                spotnum = item.brspot,
                brandnum = item.brbrand,
                amrp1 = item.amrp1,
                amrp2 = item.amrp2,
                amrp3 = item.amrp3,
                amrpsale = item.amrpsale,
                cpp = item.cpp ?? null,
                dpcoef = item.dpcoef ?? null,
                seascoef = item.seascoef ?? null,
                seccoef = item.seccoef ?? null,
                progcoef = item.progcoef ?? null,
                price = item.cena ?? null,
                status = item.status ?? null,
                chcoef = item.chcoef ?? null,
                coefA = item.koefa ?? null,
                coefB = item.koefb ?? null
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
                totalspotnum = item.totalspotbr,
                breaktype = item.breaktype,
                spotnum = Convert.ToInt32(item.brspot),
                brandnum = Convert.ToInt32(item.brbrand),
                amrp1 = item.amrp1,
                amrp2 = item.amrp2,
                amrp3 = item.amrp3,
                amrpsale = item.amrpsale,
                cpp = item.cpp ?? null,
                dpcoef = item.dpcoef ?? null,
                seascoef = item.seascoef ?? null,
                seccoef = item.seccoef ?? null,
                progcoef = item.progcoef ?? null,
                price = item.cena ?? null,
                status = item.status ?? null,
                chcoef = item.chcoef ?? null,
                coefA = item.koefa ?? null,
                coefB = item.koefb ?? null
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
                totalspotnum = item.totalspotbr,
                breaktype = item.breaktype,
                spotnum = item.brspot,
                brandnum = item.brbrand,
                amrp1 = item.amrp1,
                amrp2 = item.amrp2,
                amrp3 = item.amrp3,
                amrpsale = item.amrpsale,
                cpp = item.cpp ?? null,
                dpcoef = item.dpcoef ?? null,
                seascoef = item.seascoef ?? null,
                seccoef = item.seccoef ?? null,
                progcoef = item.progcoef ?? null,
                price = item.cena ?? null,
                status = item.status ?? null,
                chcoef = item.chcoef ?? null,
                coefA = item.koefa ?? null,
                coefB = item.koefb ?? null
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
                totalspotnum = item.totalspotbr,
                breaktype = item.breaktype,
                spotnum = item.brspot,
                brandnum = item.brbrand,
                amrp1 = item.amrp1,
                amrp2 = item.amrp2,
                amrp3 = item.amrp3,
                amrpsale = item.amrpsale,
                cpp = item.cpp ?? null,
                dpcoef = item.dpcoef ?? null,
                seascoef = item.seascoef ?? null,
                seccoef = item.seccoef ?? null,
                progcoef = item.progcoef ?? null,
                price = item.cena ?? null,
                status = item.status ?? null,
                chcoef = item.chcoef ?? null,
                coefA = item.koefa ?? null,
                coefB = item.koefb ?? null
            });

            return _mapper.Map<IEnumerable<MediaPlanRealizedDTO>>(mediaPlansRealized);
        }

        public async Task<bool> UpdateMediaPlanRealized(UpdateMediaPlanRealizedDTO mediaPlanRealizedDTO)
        {
            using var connection = _context.GetConnection();


            var affected = await connection.ExecuteAsync(
                "UPDATE xmpre SET id = @Id, cmpid = @Cmpid, naziv = @Name, vremeod = @Stime, vremedo = @Etime, " +
                " vremeodv = @Stimestr, vremedov = @Etimestr, chid = @Chid, dure = @Dure, durf = @Durf, datum = @Date, bremisije = @Emsnum, " +
                " pozinbr = @Posinbr, totalspotbr = @Totalspotnum, breaktype = @Breaktype, " +
                " brspot = @Spotnum, brbrand = @Brandnum, amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, " +
                " amrpsale = @Amrpsale, cpp = @Cpp, dpcoef = @Dpcoef, seascoef = @Seascoef, " +
                " progcoef = @Progcoef, cena = @Price, status = @Status, chcoef = @Chcoef, koefa = @CoefA, koefb = @CoefB " +
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
                    CoefB = mediaPlanRealizedDTO.coefB
                });



            return affected != 0;
        }

        public async Task<bool> UpdateMediaPlanRealized(MediaPlanRealized mediaPlanRealized)
        {
            using var connection = _context.GetConnection();


            var affected = await connection.ExecuteAsync(
                "UPDATE xmpre SET id = @Id, cmpid = @Cmpid, naziv = @Name, vremeod = @Stime, vremedo = @Etime, " +
                " vremeodv = @Stimestr, vremedov = @Etimestr, chid = @Chid, dure = @Dure, durf = @Durf, datum = @Date, bremisije = @Emsnum,  " +
                " pozinbr = @Posinbr, totalspotbr = @Totalspotnum, breaktype = @Breaktype, " +
                " brspot = @Spotnum, brbrand = @Brandnum, amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, " +
                " amrpsale = @Amrpsale, cpp = @Cpp, dpcoef = @Dpcoef, seascoef = @Seascoef, " +
                " progcoef = @Progcoef, cena = @Price, status = @Status, chcoef = @Chcoef, koefa = @CoefA, koefb = @CoefB " +
                " WHERE id = @Id",
                new
                {
                    Id = mediaPlanRealized.id,
                    Name = mediaPlanRealized.name.Trim(),
                    Cmpid = mediaPlanRealized.cmpid,
                    Stime = mediaPlanRealized.stime,
                    Etime = mediaPlanRealized.etime,
                    Stimestr = mediaPlanRealized.stimestr,
                    Etimestr = mediaPlanRealized.etimestr,
                    Chid = mediaPlanRealized.chid,
                    Dure = mediaPlanRealized.dure,
                    Durf = mediaPlanRealized.durf,
                    Date = mediaPlanRealized.date,
                    Emsnum = mediaPlanRealized.emsnum,
                    Posinbr = mediaPlanRealized.posinbr,
                    Totalspotnum = mediaPlanRealized.totalspotnum,
                    Breaktype = mediaPlanRealized.breaktype,
                    Spotnum = mediaPlanRealized.spotnum,
                    Brandnum = mediaPlanRealized.brandnum,
                    Amrp1 = mediaPlanRealized.amrp1,
                    Amrp2 = mediaPlanRealized.amrp2,
                    Amrp3 = mediaPlanRealized.amrp3,
                    Amrpsale = mediaPlanRealized.amrpsale,
                    Cpp = mediaPlanRealized.cpp,
                    Dpcoef = mediaPlanRealized.dpcoef,
                    Seascoef = mediaPlanRealized.seascoef,
                    Seccoef = mediaPlanRealized.seccoef,
                    Progcoef = mediaPlanRealized.progcoef,
                    Price = mediaPlanRealized.price,
                    Status = mediaPlanRealized.status,
                    Chcoef = mediaPlanRealized.chcoef,
                    CoefA = mediaPlanRealized.coefA,
                    CoefB = mediaPlanRealized.coefB
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
