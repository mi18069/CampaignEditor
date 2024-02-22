using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanVersionDTO;
using Database.Entities;
using System;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class MediaPlanVersionRepository : IMediaPlanVersionRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public MediaPlanVersionRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateMediaPlanVersion(MediaPlanVersionDTO mediaPlanVersionDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO cmpxmpversion (cmpid, version) " +
                "VALUES (@Cmpid, @Version) ",
            new
            {
                Cmpid = mediaPlanVersionDTO.cmpid,
                Version = mediaPlanVersionDTO.version
            });


            return affected != 0;
        }

        public async Task<MediaPlanVersionDTO> GetLatestMediaPlanVersion(int cmpid)
        {
            using var connection = _context.GetConnection();

            var mediaPlanVersion = await connection.QueryFirstOrDefaultAsync<MediaPlanVersion>(
                "SELECT * FROM cmpxmpversion WHERE cmpid = @Id", new { Id = cmpid });

            return _mapper.Map<MediaPlanVersionDTO>(mediaPlanVersion);
            
        }

        public async Task<bool> IncrementMediaPlanVersion(MediaPlanVersionDTO mediaPlanVersionDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE cmpxmpversion SET cmpid = @Cmpid, version = @Version " +
                "WHERE cmpid = @Cmpid",
                new
                {
                    Cmpid = mediaPlanVersionDTO.cmpid,
                    Version = mediaPlanVersionDTO.version + 1
                });

            return affected != 0;
        }

        public async Task<bool> UpdateMediaPlanVersion(int cmpid, int version)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE cmpxmpversion SET version = @Version WHERE cmpid = @Cmpid",
            new
            {
                    Version=version,
                    Cmpid = cmpid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanVersionById(int cmpid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM cmpxmpversion WHERE cmpid = @Id", new { Id = cmpid });

            return affected != 0;
            
        }
    }
}
