using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanRef;
using Database.Entities;
using System;
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
                "INSERT INTO tblmplper (cmpid, datestart, dateend) " +
                "VALUES (@Cmpid, @Datestart, @Dateend) ",
            new
            {
                Cmpid = mediaPlanRefDTO.cmpid,
                Datestart = mediaPlanRefDTO.datestart,
                Dateend = mediaPlanRefDTO.dateend
            });


            return affected != 0;
        }

        public async Task<MediaPlanRefDTO> GetMediaPlanRef(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlanRef = await connection.QueryFirstOrDefaultAsync<MediaPlanRef>(
                "SELECT * FROM tblmplper WHERE cmpid = @Id", 
                new { Id = id });

            return _mapper.Map<MediaPlanRefDTO>(mediaPlanRef);
        }

        public async Task<bool> UpdateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblmplper SET cmpid = @Cmpid, datestart = @Datestart, " +
                "dateend = @Dateend " +
                "WHERE cmpid = @Cmpid",
                new
                {
                    Cmpid = mediaPlanRefDTO.cmpid,
                    Datestart = mediaPlanRefDTO.datestart,
                    Dateend = mediaPlanRefDTO.dateend
                });

            return affected != 0;
        }

        public async Task<bool> DeleteMediaPlanRefById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblmplper WHERE cmpid = @Id", new { Id = id });

            return affected != 0;
        }
    }
}
