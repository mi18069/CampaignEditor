using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanRef;
using Database.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class MediaPlanRefRepository : IMediaPlanRefRepository
    {

        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public MediaPlanRefRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO cmpxmpref (cmpid, datefrom, dateto, version) " +
                "VALUES (@Cmpid, CAST (@Datefrom AS DATE), CAST (@Dateto AS DATE), @Version) ",
            new
            {
                Cmpid = mediaPlanRefDTO.cmpid,
                Datefrom = mediaPlanRefDTO.datefrom.ToString("yyyy-MM-dd"),
                Dateto = mediaPlanRefDTO.dateto.ToString("yyyy-MM-dd"),
                Version = mediaPlanRefDTO.version
            });


            return affected != 0;
        }

        public async Task<int> GetLatestRefVersionById(int id)
        {
            using var connection = _context.GetConnection();

            var latest = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT MAX(version) as latest_version FROM cmpxmpref WHERE cmpid = @Cmpid",
                new { Cmpid = id });

            return latest == null ? -1 : latest.Value;
        }

        public async Task<MediaPlanRefDTO> GetMediaPlanRefByIdAndVersion(int id, int version)
        {
            using var connection = _context.GetConnection();

            var mediaPlanRef = await connection.QueryAsync<dynamic>(
                "SELECT * FROM cmpxmpref WHERE cmpid = @Id AND version = @Version", 
                new { Id = id, Version = version });

            mediaPlanRef = mediaPlanRef.Select(item => new MediaPlanRef()
            {
                cmpid = item.cmpid,
                datefrom = DateOnly.FromDateTime(item.datefrom),
                dateto = DateOnly.FromDateTime(item.dateto),
                version = item.version
            });

            return _mapper.Map<MediaPlanRefDTO>(mediaPlanRef.FirstOrDefault());
        }

        public async Task<bool> UpdateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE cmpxmpref SET cmpid = @Cmpid, datefrom = CAST (@Datefrom AS DATE), " +
                "dateto = CAST(@Dateto AS DATE), Version = @Version " +
                "WHERE cmpid = @Cmpid AND version = @Version",
                new
                {
                    Cmpid = mediaPlanRefDTO.cmpid,
                    Datefrom = mediaPlanRefDTO.datefrom.ToString("yyyy-MM-dd"),
                    Dateto = mediaPlanRefDTO.dateto.ToString("yyyy-MM-dd"),
                    Version = mediaPlanRefDTO.version
                });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanRefById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM cmpxmpref WHERE cmpid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanRefByIdAndVersion(int id, int version)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM cmpxmpref WHERE cmpid = @Id AND version = @Version", 
                new { Id = id, Version = version });

            return affected != 0;
        }
    }
}
