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
                " cpp, dpcoef, seascoef, progcoef, cena, status " + 
                "VALUES (@Name, @Cmpid, @Stime, @Etime, @Stimestr, @Etimestr, @Chid, @Dure, @Durf, @Date, @Emsnum, " +
                " @Posinbr, @Totalspotnum, @Breaktype, @Spotnum, @Brandnum, @Amrp1, @Amrp2, @Amrp3, @Amrpsale " +
                "@Cpp, @Dpcoef, @Seascoef, @Progcoef, @Price, @Status ) ",
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
                Status = mediaPlanRealizedDTO.status
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
                name = item.naziv.Trim(),
                stime = item.vremeod,
                etime = item.vremedo,
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
                cpp = item.cpp,
                dpcoef = item.dpcoef,
                seascoef = item.seascoef,
                seccoef = item.seccoef,
                progcoef = item.progcoef,
                price = item.cena,
                status = item.status
            });

            return _mapper.Map<MediaPlanRealizedDTO>(mediaPlanRealized.FirstOrDefault());
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
                name = item.naziv.Trim(),
                stime = item.vremeod,
                etime = item.vremedo,
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
                cpp = item.cpp,
                dpcoef = item.dpcoef,
                seascoef = item.seascoef,
                seccoef = item.seccoef,
                progcoef = item.progcoef,
                price = item.cena,
                status = item.status
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
                name = item.naziv.Trim(),
                stime = item.vremeod,
                etime = item.vremedo,
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
                cpp = item.cpp,
                dpcoef = item.dpcoef,
                seascoef = item.seascoef,
                seccoef = item.seccoef,
                progcoef = item.progcoef,
                price = item.cena,
                status = item.status
            });

            return _mapper.Map<IEnumerable<MediaPlanRealizedDTO>>(mediaPlansRealized);
        }

        public async Task<bool> UpdateMediaPlanRealized(UpdateMediaPlanRealizedDTO mediaPlanRealizedDTO)
        {
            using var connection = _context.GetConnection();


            var affected = await connection.ExecuteAsync(
                "UPDATE xmpre SET id = @Id, cmpid = @Cmpid, naziv = @Name, vremeod = @Stime, vremedo = @Etime, " +
                " vremeodv = @Stimestr, vremedov = @Etimestr, chid = @Chid, dure = @Dure, durf = @Durf, datum = @Date, bremisije = @Emsnum  " +
                " pozinbr = @Posinbr, totalspotbr = @Totalspotnum, breaktype = @Breaktype, " +
                " brspot = @Spotnum, brbrand = @Brandnum, amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, " +
                " amrpsale = @Amrpsale, cpp = @Cpp, dpcoef = @Dpcoef, seascoef = @Seascoef, " +
                " progcoef = @Progcoef, cena = @Price, status = @Status " +
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
                    Status = mediaPlanRealizedDTO.status
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
    }
}
