using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanHistDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
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
                "@Date, @Progcoef, @Amr1, @Amr2, @Amr3, @Amrsale, @Amrp1, @Amrp2, @Amrp3, @Amrpsale, @Active, @Outlier) ",
            new
            {
                Xmpid = mediaPlanHistDTO.xmpid,
                Schid = mediaPlanHistDTO.schid,
                Chid = mediaPlanHistDTO.chid,
                Name = mediaPlanHistDTO.name,
                Position = mediaPlanHistDTO.position,
                Stime = mediaPlanHistDTO.stime,
                Etime = mediaPlanHistDTO.etime,
                Date = mediaPlanHistDTO.date,
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

            var mediaPlanHist = await connection.QueryFirstOrDefaultAsync<MediaPlanHist>(
                "SELECT * FROM xmphist WHERE xmphistid = @Id", new { Id = id });

            return _mapper.Map<MediaPlanHistDTO>(mediaPlanHist);
        }

        public async Task<MediaPlanHistDTO> GetMediaPlanHistByName(string name)
        {
            using var connection = _context.GetConnection();

            var mediaPlanHist = await connection.QueryFirstOrDefaultAsync<MediaPlanHist>(
                "SELECT * FROM xmphist WHERE naziv = @Name", new { Name = name });

            return _mapper.Map<MediaPlanHistDTO>(mediaPlanHist);
        }

        public async Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHists()
        {
            using var connection = _context.GetConnection();

            var allMediaPlanHists = await connection.QueryAsync<MediaPlanHist>
                ("SELECT * FROM xmphist");

            return _mapper.Map<IEnumerable<MediaPlanHistDTO>>(allMediaPlanHists);
        }

        public async Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHistsWithinDate(DateOnly sdate, DateOnly edate)
        {
            using var connection = _context.GetConnection();

            var allMediaPlanHists = await connection.QueryAsync<MediaPlanHist>
                ("SELECT * FROM xmphist WHERE datumod <= @Edate AND datumdo >= @Sdate "
                , new { Sdate = sdate, Edate = edate });

            return _mapper.Map<IEnumerable<MediaPlanHistDTO>>(allMediaPlanHists);
        }

        public async Task<IEnumerable<MediaPlanHistDTO>> GetAllChannelMediaPlanHists(int chid)
        {
            using var connection = _context.GetConnection();

            var allMediaPlanHists = await connection.QueryAsync<MediaPlanHist>
                ("SELECT * FROM xmphist WHERE chid=@Chid", new { Chid = chid });

            return _mapper.Map<IEnumerable<MediaPlanHistDTO>>(allMediaPlanHists);
        }

        public async Task<bool> UpdateMediaPlanHist(UpdateMediaPlanHistDTO mediaPlanHistDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE xmphist SET xmphistid = @Xmphistid, xmpid = @Xmpid, schid = @Schid, chid = @Chid, naziv = @Name, " +
                "pozicija = @Position,vremeod = @Stime, vremedo = @Etime, datum = @Date, progkoef = @Progcoef " +
                "amr1 = @Amr1, amr2 = @Amr2, amr3 = @Amr3, amrsale = @Amrsale, amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, " +
                "amr1 = @Amr1,amrpsale = @Amrpsale, active = @Active, outlier = @Outlier " +
                "WHERE xmphistid = @Xmphistid",
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
                    Date = mediaPlanHistDTO.date,
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
    }
}
