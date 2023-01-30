using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.TargetValueDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class TargetValueRepository : ITargetValueRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public TargetValueRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateTargetValue(CreateTargetValueDTO targetValueDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO demo_slave (name, value) " +
                "VALUES (@TargetValueName, @TargetValueValue)",
            new
            {
                TargetValueName = targetValueDTO.name,
                TargetValueValue = targetValueDTO.value
            });

            return affected != 0;
        }

        public async Task<TargetValueDTO> GetTargetValueById(int id)
        {
            using var connection = _context.GetConnection();

            var targetValue = await connection.QueryFirstOrDefaultAsync<TargetValue>(
                "SELECT * FROM demo_slave WHERE demoid = @Id", new { Id = id });

            return _mapper.Map<TargetValueDTO>(targetValue);
        }

        public async Task<TargetValueDTO> GetTargetValueByName(string valuename)
        {
            using var connection = _context.GetConnection();

            var targetValue = await connection.QueryFirstOrDefaultAsync<TargetValue>(
                "SELECT * FROM demo_slave WHERE name = @Valuename", new { Valuename = valuename });

            return _mapper.Map<TargetValueDTO>(targetValue);
        }
        public async Task<IEnumerable<TargetValueDTO>> GetAllTargetValues()
        {
            using var connection = _context.GetConnection();

            var allTargetValues = await connection.QueryAsync<TargetValue>("SELECT * FROM demo_slave");

            return _mapper.Map<IEnumerable<TargetValueDTO>>(allTargetValues);
        }

        public async Task<IEnumerable<TargetValueDTO>> GetAllTargetValuesWithId(int id)
        {
            using var connection = _context.GetConnection();

            var allTargetValues = await connection.QueryAsync<TargetValue>
                ("SELECT * FROM demo_slave WHERE demoid = @Id", new { Id = id });

            return _mapper.Map<IEnumerable<TargetValueDTO>>(allTargetValues);
        }

        public async Task<bool> UpdateTargetValue(UpdateTargetValueDTO targetValueDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "Update demo_slave SET demoid = @TargetValueId, name = @TargetValueName, " +
                "value = @TargetValueValue" +
                "WHERE demoid = @TargetValueId",
            new
            {
                TargetValueId = targetValueDTO.id,
                TargetValueName = targetValueDTO.name,
                TargetValueValue = targetValueDTO.value
            });

            return affected != 0;
        }
        public async Task<bool> DeleteTargetValueById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM demo_slave WHERE demoid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteTargetValueByName(string valuename)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM demo_slave WHERE demo = @Valuename", new { Valuename = valuename });

            return affected != 0;
        }
    }
}
