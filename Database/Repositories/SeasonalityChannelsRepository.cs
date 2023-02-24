using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.SeasonalityChannelsDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SeasonalityChannelsRepository : ISeasonalityChannelsRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public SeasonalityChannelsRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateSeasonalityChannels(CreateSeasonalityChannelsDTO seasonalityChannelsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblseasonalitychn (seasid, chid)" +
                    "VALUES (@Seasid, @Chid)",
            new
            {
                Seasid = seasonalityChannelsDTO.seasid,
                Chid = seasonalityChannelsDTO.chid
             });

            return affected != 0;
        }

        public async Task<SeasonalityChannelsDTO> GetSeasonalityChannelsByIds(int seasid, int chid)
        {
            using var connection = _context.GetConnection();

            var seasonalityChannels = await connection.QueryFirstOrDefaultAsync<SectableChannels>(
                "SELECT * FROM tblseasonalitychn WHERE seasid = @Seasid AND chid = @Chid",
                new
                {
                    Seasid = seasid,
                    Chid = chid
                });

            return _mapper.Map<SeasonalityChannelsDTO>(seasonalityChannels);
        }

        public async Task<IEnumerable<SeasonalityChannelsDTO>> GetAllSeasonalityChannelsBySeasid(int seasid)
        {
            using var connection = _context.GetConnection();

            var seasonalityChannels = await connection.QueryAsync<SeasonalityChannels>
                ("SELECT * FROM tblseasonalitychn WHERE seasid = @Seasid", new { Seasid = seasid });

            return _mapper.Map<IEnumerable<SeasonalityChannelsDTO>>(seasonalityChannels);
        }
        public async Task<IEnumerable<SeasonalityChannelsDTO>> GetAllSeasonalityChannelsByChid(int chid)
        {
            using var connection = _context.GetConnection();

            var seasonalityChannels = await connection.QueryAsync<SeasonalityChannels>
                ("SELECT * FROM tblseasonalitychn WHERE chid = @Chid", new { Chid = chid });

            return _mapper.Map<IEnumerable<SeasonalityChannelsDTO>>(seasonalityChannels);
        }
        public async Task<IEnumerable<SeasonalityChannelsDTO>> GetAllSeasonalityChannels()
        {
            using var connection = _context.GetConnection();

            var seasonalityChannels = await connection.QueryAsync<SeasonalityChannels>
                ("SELECT * FROM tblseasonalitychn ");

            return _mapper.Map<IEnumerable<SeasonalityChannelsDTO>>(seasonalityChannels);
        }

        public async Task<bool> UpdateSeasonalityChannels(UpdateSeasonalityChannelsDTO seasonalityChannelsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblseasonalitychn SET seasid = @Seasid, chid = @Chid" +
                "WHERE seasid = @Seasid AND chid = @Chid",
                new
                {
                    Seasid = seasonalityChannelsDTO.seasid,
                    Chid = seasonalityChannelsDTO.chid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteSeasonalityChannelsByIds(int seasid, int chid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblseasonalitychn WHERE seasid = @Seasid AND chid = @Chid",
                new
                {
                    Seasid = seasid,
                    Chid = chid
                });

            return affected != 0;
        }
    }
}
