using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.SeasonalityDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SeasonalityRepository : ISeasonalityRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public SeasonalityRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateSeasonality(CreateSeasonalityDTO seasonalityDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblseasonality (seasname, seasactive, channel, ownedby)" +
                    "VALUES (@Seasname, @Seasactive, @Ownedby)",
            new
            {
                Sctname = seasonalityDTO.seasname,
                Seasactive = seasonalityDTO.seasactive,
                Ownedby = seasonalityDTO.ownedby
            });

            return affected != 0;
        }

        public async Task<SeasonalityDTO> GetSeasonalityById(int id)
        {
            using var connection = _context.GetConnection();

            var seasonality = await connection.QueryFirstOrDefaultAsync<Sectable>(
                "SELECT * FROM tblseasonality WHERE seasid = @Id", new { Id = id });

            return _mapper.Map<SeasonalityDTO>(seasonality);
        }
        public async Task<SeasonalityDTO> GetSeasonalityByName(string seasonalityname)
        {
            using var connection = _context.GetConnection();

            var seasonality = await connection.QueryFirstOrDefaultAsync<Channel>(
                "SELECT * FROM tblseasonality WHERE seasname = @Seasname", new { Seasname = seasonalityname });

            return _mapper.Map<SeasonalityDTO>(seasonality);
        }
        public async Task<IEnumerable<SeasonalityDTO>> GetAllSeasonalitiesByOwnerId(int id)
        {
            using var connection = _context.GetConnection();

            var allSeasonalities = await connection.QueryAsync<Seasonality>
                ("SELECT * FROM tblseasonality WHERE ownedby = @Id OR ownedby = 0", new { Id = id });

            return _mapper.Map<IEnumerable<SeasonalityDTO>>(allSeasonalities);
        }
        public async Task<IEnumerable<SeasonalityDTO>> GetAllSeasonalities()
        {
            using var connection = _context.GetConnection();

            var allSeasonalities = await connection.QueryAsync<Seasonality>("SELECT * FROM tblseasonality");

            return _mapper.Map<IEnumerable<SeasonalityDTO>>(allSeasonalities);
        }

        public async Task<bool> UpdateSeasonality(UpdateSeasonalityDTO seasonalityDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblseasonality SET seasid = @Seasid, seasname = @Seasname, " +
                "ownedby = @Ownedby" +
                "WHERE sctid = @Sctid",
            new
            {
                    Seasid = seasonalityDTO.seasid,
                    Seasname = seasonalityDTO.seasname,
                    Ownedby = seasonalityDTO.ownedby
                });

            return affected != 0;
        }
        public async Task<bool> DeleteSeasonalityById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblseasonality WHERE seasid = @Sctid", new { Seasid = id });

            return affected != 0;
        }


    }
}
