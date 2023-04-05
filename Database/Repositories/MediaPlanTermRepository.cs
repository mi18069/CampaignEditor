using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanHistDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
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
        public async Task<bool> CreateMediaPlanTerm(CreateMediaPlanTermDTO mediaPlanTermDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO xmpterm (xmpid, datum, spotcode) " +
                "VALUES (@Xmpid, @Date, @Spotcode) ",
            new
            {
                Xmpid = mediaPlanTermDTO.xmpid,
                Date = mediaPlanTermDTO.date,
                Spotcode = mediaPlanTermDTO.spotcode
            });


            return affected != 0;
        }

        public async Task<MediaPlanTermDTO> GetMediaPlanTermById(int id)
        {
            using var connection = _context.GetConnection();

            var mediaPlanTerm = await connection.QueryFirstOrDefaultAsync<MediaPlanTerm>(
                "SELECT * FROM xmpterm WHERE xmptermid = @Id", new { Id = id });

            return _mapper.Map<MediaPlanTermDTO>(mediaPlanTerm);
        }

        public async Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTermsByXmpid(int xmpid)
        {
            using var connection = _context.GetConnection();

            var allMediaPlanTerms = await connection.QueryFirstOrDefaultAsync<MediaPlanTerm>(
                "SELECT * FROM xmpterm WHERE xmpid = @Xmpid", new { Xmpid = xmpid });

            return _mapper.Map<IEnumerable<MediaPlanTermDTO>>(allMediaPlanTerms);
        }

        public async Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTerms()
        {
            using var connection = _context.GetConnection();

            var allMediaPlanTerms = await connection.QueryAsync<MediaPlanTerm>
                ("SELECT * FROM xmpterm");

            return _mapper.Map<IEnumerable<MediaPlanTermDTO>>(allMediaPlanTerms);
        }

        public async Task<bool> UpdateMediaPlanTerm(UpdateMediaPlanTermDTO mediaPlanTermDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE xmpterm SET xmptermid = @Xmptermid, xmpid = @Xmpid, datum = @Date, spotcode = @Spotcode " +
                "WHERE xmptermid = @Xmptermid",
                new
                {
                    Xmpterm = mediaPlanTermDTO.xmptermid,
                    Xmpid = mediaPlanTermDTO.xmpid,
                    Date = mediaPlanTermDTO.date,
                    Spotcode = mediaPlanTermDTO.spotcode
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
    }
}
