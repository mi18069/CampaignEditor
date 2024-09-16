using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanTermDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class MediaPlanTermRepository : IMediaPlanTermRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public MediaPlanTermRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> SetTermSerialNumber()
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                @"
                    DO $$
                    DECLARE
                        max_xmptermid INTEGER;
                    BEGIN
                        SELECT MAX(xmptermid) INTO max_xmptermid FROM xmpterm;
    
                        -- Step 2: Set the sequence value to the next value after the maximum xmptermid
                        EXECUTE format('ALTER SEQUENCE xmpterm_xmptermid_seq RESTART WITH %s', max_xmptermid + 1);
                    END $$;
                ");


            return affected != 0;
        }
        public async Task<bool> CreateMediaPlanTerm(CreateMediaPlanTermDTO mediaPlanTermDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO xmpterm (xmpid, datum, spotcode, added, deleted) " +
                "VALUES (@Xmpid, CAST (@Date AS DATE), @Spotcode, @Added, @Deleted) ",
            new
            {
                Xmpid = mediaPlanTermDTO.xmpid,
                Date = mediaPlanTermDTO.date.ToString("yyyy-MM-dd"),
                Spotcode = mediaPlanTermDTO.spotcode,
                Added = mediaPlanTermDTO.added,
                Deleted = mediaPlanTermDTO.deleted
            });


            return affected != 0;
        }

        public async Task<MediaPlanTermDTO> GetMediaPlanTermById(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlanTerm = await connection.QueryFirstOrDefaultAsync<MediaPlanTerm>(
                "SELECT * FROM xmpterm WHERE xmptermid = @Id", new { Id = id });

            if (mediaPlanTerm != null)
            {
                mediaPlanTerm.Spotcode = mediaPlanTerm.Spotcode?.Trim();
            }

            return _mapper.Map<MediaPlanTermDTO>(mediaPlanTerm);
        }

        public async Task<MediaPlanTermDTO> GetMediaPlanTermByXmpidAndDate(int id, DateOnly date)
        {
            using var connection = _context.GetConnection();

            var mediaPlanTerm = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmpterm WHERE xmpid = @Id AND datum= CAST(@Date AS DATE)", 
                new { Id = id, Date=date.ToString("yyyy-MM-dd") });

            mediaPlanTerm = mediaPlanTerm.Select(item => new MediaPlanTerm()
            {
                Xmptermid = item.xmptermid,
                Xmpid = item.xmpid,
                Date = DateOnly.FromDateTime(item.datum),
                Spotcode = item.spotcode != null ? item.spotcode.Trim() : null
            });

            return _mapper.Map<MediaPlanTermDTO>(mediaPlanTerm.First());
        }

        public async Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTermsByXmpid(int xmpid)
        {
            using var connection = _context.GetConnection();

            var allMediaPlanTerms = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmpterm WHERE xmpid = @Xmpid ORDER BY datum", new { Xmpid = xmpid });

            allMediaPlanTerms = allMediaPlanTerms.Select(item => new MediaPlanTerm()
            {
                Xmptermid = item.xmptermid,
                Xmpid = item.xmpid,
                Date = DateOnly.FromDateTime(item.datum),
                Spotcode = item.spotcode != null ? item.spotcode.Trim() : null
            });

            return _mapper.Map<IEnumerable<MediaPlanTermDTO>>(allMediaPlanTerms);
        }

        public async Task<IEnumerable<MediaPlanTermDTO>> GetAllNotNullMediaPlanTermsByXmpid(int xmpid)
        {
            using var connection = _context.GetConnection();

            var allMediaPlanTerms = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xmpterm WHERE xmpid = @Xmpid AND spotcode IS NOT NULL ORDER BY datum", new { Xmpid = xmpid });

            allMediaPlanTerms = allMediaPlanTerms.Select(item => new MediaPlanTerm()
            {
                Xmptermid = item.xmptermid,
                Xmpid = item.xmpid,
                Date = DateOnly.FromDateTime(item.datum),
                Spotcode = item.spotcode != null ? item.spotcode.Trim() : null
            });

            return _mapper.Map<IEnumerable<MediaPlanTermDTO>>(allMediaPlanTerms);
        }

        public async Task<bool> CheckIfMediaPlanHasSpotsDedicated(int xmpid)
        {
            using var connection = _context.GetConnection();

            // Get the count of terms with spots dedicated for the specified xmpid
            var termCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM xmpterm WHERE xmpid = @Xmpid AND (spotcode IS NOT NULL AND spotcode != '')",
                new { Xmpid = xmpid });

            // Check if the count is 0
            bool hasSpotsDedicated = termCount > 0;

            return hasSpotsDedicated;
        }

        public async Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTerms()
        {
            using var connection = _context.GetConnection();

            var allMediaPlanTerms = await connection.QueryAsync<MediaPlanTerm>
                ("SELECT * FROM xmpterm");

            foreach (var mediaPlanTerm in allMediaPlanTerms)
                if (mediaPlanTerm != null)
                {
                    mediaPlanTerm.Spotcode = mediaPlanTerm.Spotcode?.Trim();
                }

            return _mapper.Map<IEnumerable<MediaPlanTermDTO>>(allMediaPlanTerms);
        }

        public async Task<bool> UpdateMediaPlanTerm(UpdateMediaPlanTermDTO mediaPlanTermDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE xmpterm SET xmptermid = @Xmptermid, xmpid = @Xmpid, datum = CAST(@Date AS DATE), " +
                "spotcode = @Spotcode, added = @Added, deleted = @Deleted " +
                "WHERE xmptermid = @Xmptermid",
                new
                {
                    Xmptermid = mediaPlanTermDTO.xmptermid,
                    Xmpid = mediaPlanTermDTO.xmpid,
                    Date = mediaPlanTermDTO.date.ToString("yyyy-MM-dd"),
                    Spotcode = mediaPlanTermDTO.spotcode,
                    Added = mediaPlanTermDTO.added,
                    Deleted = mediaPlanTermDTO.deleted
                });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanTermById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM xmpterm WHERE xmptermid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanTermByXmpId(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM xmpterm WHERE xmpid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> SetActiveMediaPlanTermByMPId(int id, bool isActive)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE xmpterm SET active = @Active " +
                "WHERE xmpid = @Id", new { Active = isActive, Id = id });

            return affected != 0;
        }
    }
}
