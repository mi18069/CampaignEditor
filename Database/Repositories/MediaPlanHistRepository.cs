using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanHistDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class MediaPlanHistRepository : IMediaPlanHistRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public MediaPlanHistRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateMediaPlanHist(CreateMediaPlanHistDTO mediaPlanHistDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO xmphsit (xmpid, schid, chid, naziv, pozicija, vremeod, vremedo, " +
                "datum, progkoef, amr1, amr2, amr3, amrsale, amrp1, amrp2, amrp3, amrpsale, active, outlier) " +
                "VALUES (@Xmpid, @Schid, @Chid, @Name, @Position, @Stime, @Etime, " +
                "CAST(@Date AS DATE), @Progcoef, @Amr1, @Amr2, @Amr3, @Amrsale, @Amrp1, @Amrp2, @Amrp3, @Amrpsale, @Active, @Outlier) ",
            new
            {
                Xmpid = mediaPlanHistDTO.xmpid,
                Schid = mediaPlanHistDTO.schid,
                Chid = mediaPlanHistDTO.chid,
                Name = mediaPlanHistDTO.name,
                Position = mediaPlanHistDTO.position,
                Stime = mediaPlanHistDTO.stime,
                Etime = mediaPlanHistDTO.etime,
                Date = mediaPlanHistDTO.date.ToString("yyyy-MM-dd"),
                Progcoef = mediaPlanHistDTO.progcoef,
                Amr1 = mediaPlanHistDTO.amr1,
                Amr2 = mediaPlanHistDTO.amr2,
                Amr3 = mediaPlanHistDTO.amr3,
                Amrsale = mediaPlanHistDTO.amrsale,
                Amrp1 = mediaPlanHistDTO.amrp1,
                Amrp2 = mediaPlanHistDTO.amrp2,
                Amrp3 = mediaPlanHistDTO.amrp3,
                Amrpsale = mediaPlanHistDTO.amrpsale,
                Active = mediaPlanHistDTO.active,
                Outlier = mediaPlanHistDTO.outlier
            });


            return affected != 0;
        }

        public async Task<MediaPlanHistDTO> GetMediaPlanHistById(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlanHist = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmphist WHERE xmphistid = @Id AND amrsale is not null", new { Id = id });

            mediaPlanHist = mediaPlanHist.Select(item => new MediaPlanHist()
            {
                xmphistid = item.xmphistid,
                xmpid = item.xmpid,
                schid = item.schid,
                chid = item.chid,
                name = item.naziv,
                position = item.pozicija,
                stime = item.vremeod,
                etime = item.vremedo == null ? null : item.vremedo,
                date = item.datum,
                progcoef = (float)item.progkoef,
                amr1 = (double)item.amr1,
                amr2 = (double)item.amr2,
                amr3 = (double)item.amr3,
                amrsale = (double)item.amrsale,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                active = item.active,
                outlier = item.outlier
            });

            return _mapper.Map<IEnumerable<MediaPlanHistDTO>>(mediaPlanHist.FirstOrDefault());
        }

        public async Task<IEnumerable<MediaPlanHist>> GetAllMediaPlanHistsByXmpid(int xmpid)
        {
            using var connection = _context.GetConnection();

            var allMediaPlanHists = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmphist WHERE xmpid = @Xmpid AND amrsale is not null", new { Xmpid = xmpid });

            allMediaPlanHists = allMediaPlanHists.Select(item => new MediaPlanHist()
            {
                xmphistid = item.xmphistid,
                xmpid = item.xmpid,
                schid = item.schid,
                chid = item.chid,
                name = item.naziv,
                position = item.pozicija,
                stime = item.vremeod,
                etime = item.vremedo == null ? null : item.vremedo,
                date = DateOnly.FromDateTime(item.datum),
                progcoef = (float)item.progkoef,
                amr1 = (double)item.amr1,
                amr2 = (double)item.amr2,
                amr3 = (double)item.amr3,
                amrsale = (double)item.amrsale,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                active = item.active,
                outlier = item.outlier
            });

            return (IEnumerable<MediaPlanHist>)allMediaPlanHists;
        }

        public async Task<IEnumerable<MediaPlanHist>> GetAllMediaPlanHistsBySchid(int schid)
        {
            using var connection = _context.GetConnection();

            var allMediaPlanHists = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmphist WHERE schid = @Schid AND amrsale is not null", new { Schid = schid });

            allMediaPlanHists = allMediaPlanHists.Select(item => new MediaPlanHist()
            {
                xmphistid = item.xmphistid,
                xmpid = item.xmpid,
                schid = item.schid,
                chid = item.chid,
                name = item.naziv,
                position = item.pozicija,
                stime = item.vremeod,
                etime = item.vremedo == null ? null : item.vremedo,
                date = DateOnly.FromDateTime(item.datum),
                progcoef = (float)item.progkoef,
                amr1 = (double)item.amr1,
                amr2 = (double)item.amr2,
                amr3 = (double)item.amr3,
                amrsale = (double)item.amrsale,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                active = item.active,
                outlier = item.outlier
            });

            return (IEnumerable<MediaPlanHist>)allMediaPlanHists;
        }

        public async Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHists()
        {
            using var connection = _context.GetConnection();

            var allMediaPlanHists = await connection.QueryAsync<dynamic>
                ("SELECT * FROM xmphist WHERE amrsale is not null");

            allMediaPlanHists = allMediaPlanHists.Select(item => new MediaPlanHist()
            {
                xmphistid = item.xmphistid,
                xmpid = item.xmpid,
                schid = item.schid,
                chid = item.chid,
                name = item.naziv,
                position = item.pozicija,
                stime = item.vremeod,
                etime = item.vremedo == null ? null : item.vremedo,
                date = DateOnly.FromDateTime(item.datum),
                progcoef = (float)item.progkoef,
                amr1 = (double)item.amr1,
                amr2 = (double)item.amr2,
                amr3 = (double)item.amr3,
                amrsale = (double)item.amrsale,
                amrp1 = (double)item.amrp1,
                amrp2 = (double)item.amrp2,
                amrp3 = (double)item.amrp3,
                amrpsale = (double)item.amrpsale,
                active = item.active,
                outlier = item.outlier
            });

            return _mapper.Map<IEnumerable<MediaPlanHistDTO>>(allMediaPlanHists);
        }

        public async Task<IEnumerable<MediaPlanHistDTO>> GetAllChannelMediaPlanHists(int chid)
        {
            using var connection = _context.GetConnection();

            var allMediaPlanHists = await connection.QueryAsync<MediaPlanHist>
                ("SELECT * FROM xmphist WHERE chid=@Chid AND amrsale is not null", new { Chid = chid });

            return _mapper.Map<IEnumerable<MediaPlanHistDTO>>(allMediaPlanHists);
        }

        public async Task<bool> UpdateMediaPlanHist(UpdateMediaPlanHistDTO mediaPlanHistDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE xmphist SET xmphistid = @Xmphistid, xmpid = @Xmpid, schid = @Schid, chid = @Chid, naziv = @Name, " +
                "pozicija = @Position,vremeod = @Stime, vremedo = @Etime, datum = CAST(@Date AS DATE), progkoef = @Progcoef, " +
                "amr1 = @Amr1, amr2 = @Amr2, amr3 = @Amr3, amrsale = @Amrsale, amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, " +
                "amrpsale = @Amrpsale, active = @Active, outlier = @Outlier " +
                "WHERE xmphistid = @Xmphistid AND amrsale is not null",
                new
                {
                    Xmphistid = mediaPlanHistDTO.xmphistid,
                    Xmpid = mediaPlanHistDTO.xmpid,
                    Schid = mediaPlanHistDTO.schid,
                    Chid = mediaPlanHistDTO.chid,
                    Name = mediaPlanHistDTO.name,
                    Position = mediaPlanHistDTO.position,
                    Stime = mediaPlanHistDTO.stime,
                    Etime = mediaPlanHistDTO.etime,
                    Date = mediaPlanHistDTO.date.ToString("yyyy-MM-dd"),
                    Progcoef = mediaPlanHistDTO.progcoef,
                    Amr1 = mediaPlanHistDTO.amr1,
                    Amr2 = mediaPlanHistDTO.amr2,
                    Amr3 = mediaPlanHistDTO.amr3,
                    Amrsale = mediaPlanHistDTO.amrsale,
                    Amrp1 = mediaPlanHistDTO.amrp1,
                    Amrp2 = mediaPlanHistDTO.amrp2,
                    Amrp3 = mediaPlanHistDTO.amrp3,
                    Amrpsale = mediaPlanHistDTO.amrpsale,
                    Active = mediaPlanHistDTO.active,
                    Outlier = mediaPlanHistDTO.outlier
                });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanHistById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM xmphist WHERE xmphistid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanHistByXmpid(int xmpid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM xmphist WHERE xmpid = @Xmpid", new { Xmpid = xmpid });

            return affected != 0;
        }

        public MediaPlanHist ConvertFromDTO(MediaPlanHistDTO mediaPlanHistDTO)
        {
            return _mapper.Map<MediaPlanHist>(mediaPlanHistDTO);
        }

        public MediaPlanHistDTO ConvertToDTO(MediaPlanHist mediaPlanHist)
        {
            return _mapper.Map<MediaPlanHistDTO>(mediaPlanHist);
        }
    }
}
