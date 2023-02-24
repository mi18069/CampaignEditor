using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.SeasonalitiesDTO;
using Database.DTOs.SectableDTO;
using Database.DTOs.SectablesDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SeasonalitiesRepository : ISeasonalitiesRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public SeasonalitiesRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateSeasonalities(CreateSeasonalitiesDTO seasonalitiesDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblseasonalities (seasid, stdt, endt, coef)" +
                    "VALUES (@Seasid, @Stdt, @Endt, @Coef)",
            new
            {
                Seasid = seasonalitiesDTO.seasid,
                Stdt = seasonalitiesDTO.stdt,
                Endt = seasonalitiesDTO.endt,
                Coef = seasonalitiesDTO.coef
            });

            return affected != 0;
        }
        public async Task<IEnumerable<SeasonalitiesDTO>> GetSeasonalitiesById(int id)
        {
            using var connection = _context.GetConnection();

            var seasonalities = await connection.QueryFirstOrDefaultAsync<Sectables>(
                "SELECT * FROM tblseasonalities WHERE seasid = @Id", new { Id = id });

            return _mapper.Map<IEnumerable<SeasonalitiesDTO>>(seasonalities);
        }

        public async Task<IEnumerable<SeasonalitiesDTO>> GetAllSeasonalities()
        {
            using var connection = _context.GetConnection();

            var allSeasonalities = await connection.QueryAsync<Seasonalities>("SELECT * FROM tblseasonalities");

            return _mapper.Map<IEnumerable<SeasonalitiesDTO>>(allSeasonalities);
        }

        public async Task<bool> UpdateSeasonalities(UpdateSeasonalitiesDTO seasonalitiesDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblseasonalities SET seasid = @Seasid, stdt = @Stdt, endt = @Endt, coef = @Coef" +
                "WHERE seasid = @Seasid",
            new
            {
                Seasid = seasonalitiesDTO.seasid,
                Stdt = seasonalitiesDTO.stdt,
                Endt = seasonalitiesDTO.endt,
                Coef = seasonalitiesDTO.coef
            });

            return affected != 0;
        }

        public async Task<bool> DeleteSeasonalitiesById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblseasonalities WHERE seasid = @Seasid", new { Seasid = id });

            return affected != 0;
        }
    }
}
