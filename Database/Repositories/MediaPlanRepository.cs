using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                "amr1, amr2, amr3, amrsale, amrp1, amrp2, amrp3, amrpsale, dpkoef, seaskoef, price, active) " +
                "VALUES (@Schid, @Cmpid, @Chid, @Name, @Version, @Position, @Stime, @Etime, @Blocktime, " +
                "@Days, @Type, @Special, CAST (@Sdate AS DATE), CAST(@Edate AS DATE), @Progcoef, CAST(@Created AS DATE), CAST(@Modified AS DATE), " +
                "@Amr1, @Amr2, @Amr3, @Amrsale, @Amrp1, @Amrp2, @Amrp3, @Amrpsale, @Dpcoef, @Seascoef, @Price, @Active) ",
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
                Amr2 = mediaPlanDTO.amr2,
                Amr3 = mediaPlanDTO.amr3,
                Amrsale = mediaPlanDTO.amrsale,
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

            var mediaPlan = await connection.QueryFirstOrDefaultAsync<MediaPlan>(
                "SELECT * FROM xmp WHERE id = @Id", new { Id = id });

            return _mapper.Map<MediaPlanDTO>(mediaPlan);
        }

        public async Task<MediaPlanDTO> GetMediaPlanBySchemaId(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryFirstOrDefaultAsync<MediaPlan>(
                "SELECT * FROM xmp WHERE schid = @Id", new { Id = id });

            return _mapper.Map<MediaPlanDTO>(mediaPlan);
        }

        public async Task<MediaPlanDTO> GetMediaPlanBySchemaAndCmpId(int schemaid, int cmpid)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryFirstOrDefaultAsync<MediaPlan>(
                "SELECT * FROM xmp WHERE schid = @Schemaid AND cmpid = @Cmpid", 
                new { Schemaid = schemaid, Cmpid = cmpid });

            return _mapper.Map<MediaPlanDTO>(mediaPlan);
        }

        public async Task<MediaPlanDTO> GetMediaPlanByCmpId(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryFirstOrDefaultAsync<MediaPlan>(
                "SELECT * FROM xmp WHERE cmpid = @Cmpid", new { Cmpid = id });

            return _mapper.Map<MediaPlanDTO>(mediaPlan);
        }

        public async Task<MediaPlanDTO> GetMediaPlanByName(string name)
        {
            using var connection = _context.GetConnection();

            var mediaPlan = await connection.QueryFirstOrDefaultAsync<MediaPlan>(
                "SELECT * FROM xmp WHERE naziv = @Name", new { Name = name });

            return _mapper.Map<MediaPlanDTO>(mediaPlan);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlans()
        {
            using var connection = _context.GetConnection();

            var allMediaPlans = await connection.QueryAsync<MediaPlan>
                ("SELECT * FROM xmp");

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(allMediaPlans);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansWithinDate(DateOnly sdate, DateOnly edate)
        {
            using var connection = _context.GetConnection();

            var allMediaPlans = await connection.QueryAsync<MediaPlan>
                ("SELECT * FROM xmp WHERE datumod <= @Edate AND datumdo >= @Sdate "
                , new { Sdate=sdate, Edate=edate});

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(allMediaPlans);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllChannelMediaPlans(int chid)
        {
            using var connection = _context.GetConnection();

            var allMediaPlans = await connection.QueryAsync<MediaPlan>
                ("SELECT * FROM xmp WHERE chid=@Chid", new { Chid = chid });

            return _mapper.Map<IEnumerable<MediaPlanDTO>>(allMediaPlans);
        }

        public async Task<bool> UpdateMediaPlan(UpdateMediaPlanDTO mediaPlanDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE xmp SET xmpid = @Xmpid, schid = @Schid, cmpid = @Cmpid, chid = @Chid, naziv = @Name, " +
                "verzija = @Version, pozicija = @Position, " +
                "vremeod = @Stime, vremedo = @Etime, vremerbl = @Blocktime, dani = @Days, " +
                "tipologija = @Type, specijal = @Special, datumod = @Sdate, datumdo = @Edate, " +
                "datumkreiranja = @Created, datumizmene = @Modified " +
                "amr1 = @Amr1, amr2 = @Amr2, amr3 = @Amr3, amrsale = @Amrsale, amrp1 = @Amrp1, amrp2 = @Amrp2, amrp3 = @Amrp3, " +
                "amr1 = @Amr1,amrpsale = @Amrpsale, dpkoef = @Dpcoef, seaskoef = @Seascoef, price = @Price, active = @Active " +
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
                    Sdate = mediaPlanDTO.sdate,
                    Edate = mediaPlanDTO.edate,
                    Progcoef = mediaPlanDTO.progcoef,
                    Created = mediaPlanDTO.created,
                    Modified = mediaPlanDTO.modified,
                    Amr1 = mediaPlanDTO.amr1,
                    Amr2 = mediaPlanDTO.amr2,
                    Amr3 = mediaPlanDTO.amr3,
                    Amrsale = mediaPlanDTO.amrsale,
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
