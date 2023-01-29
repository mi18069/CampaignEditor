using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.TargetClassDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class TargetClassRepository : ITargetClassRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public TargetClassRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateTargetClass(CreateTargetClassDTO targetClassDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO demo_master (demo, type, position) " +
                "VALUES (@TargetClassName, @TargetClassType, @TargetClassPosition)",
            new
            {
                TargetClassName = targetClassDTO.name,
                TargetClassType = targetClassDTO.type,
                TargetClassPosition = targetClassDTO.position
            }) ;

            return affected != 0;
        }

        public async Task<TargetClassDTO> GetTargetClassById(int id)
        {
            using var connection = _context.GetConnection();

            var targetClass = await connection.QueryFirstOrDefaultAsync<TargetClass>(
                "SELECT * FROM demo_master WHERE demoid = @Id", new { Id = id });

            return _mapper.Map<TargetClassDTO>(targetClass);
        }

        public async Task<TargetClassDTO> GetTargetClassByName(string classname)
        {
            using var connection = _context.GetConnection();

            var targetClass = await connection.QueryFirstOrDefaultAsync<TargetClass>(
                "SELECT * FROM demo_master WHERE demo = @Classname", new { Classname = classname });

            return _mapper.Map<TargetClassDTO>(targetClass);
        }

        public async Task<IEnumerable<TargetClassDTO>> GetAllTargetClasses()
        {
            using var connection = _context.GetConnection();

            var allTargetClasses = await connection.QueryAsync<TargetClass>("SELECT * FROM demo_master");

            return _mapper.Map<IEnumerable<TargetClassDTO>>(allTargetClasses);
        }

        public async Task<bool> UpdateTargetClass(UpdateTargetClassDTO targetClassDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "Update demo_master SET demoid = @TargetClassId, demo = @TargetClassName, " +
                "type = @TargetClassType, position = @TargetClassPosition) " +
                "WHERE demoid = @TargetClassId",
            new
            {
                TargetClassId = targetClassDTO.classid,
                TargetClassName = targetClassDTO.name,
                TargetClassType = targetClassDTO.type,
                TargetClassPosition = targetClassDTO.position,
            });

            return affected != 0;
        }

        public async Task<bool> DeleteTargetClassById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM demo_master WHERE demoid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteTargetClassByName(string classname)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM demo_master WHERE demo = @Classname", new { Classname = classname });

            return affected != 0;
        }

    }
}
