using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.DayPartDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class DayPartRepository : IDayPartRepository
    {

        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public DayPartRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateDayPart(CreateDayPartDTO dayPartDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tbldp (clid, name, days) " +
                "VALUES (@Clid, @Name, @Days)",
            new
            {
                Clid = dayPartDTO.clid,
                Name = dayPartDTO.name,
                Days = dayPartDTO.days
            });

            return affected != 0;
        }
        public async Task<DayPartDTO> GetDayPartById(int id)
        {
            using var connection = _context.GetConnection();

            var dayPart = await connection.QueryFirstOrDefaultAsync<DayPart>(
                "SELECT * FROM tbldp WHERE dpid = @Id", new { Id = id });

            return _mapper.Map<DayPartDTO>(dayPart);
        }

        public async Task<IEnumerable<DayPartDTO>> GetAllClientDayParts(int clientId)
        {
            using var connection = _context.GetConnection();

            var allDayParts = await connection.QueryAsync<DayPart>(
                "SELECT * FROM tbldp WHERE clid = @ClientId",
                new { ClientId = clientId });

            return _mapper.Map<IEnumerable<DayPartDTO>>(allDayParts);
        }

        public async Task<bool> UpdateDayPart(UpdateDayPartDTO dayPartDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "Update tbldp SET dpid = @Dpid, clid = @Clid, " +
                "name = @Name, days = @Days " +
                "WHERE dpid = @Dpid",
            new
            {
                Dpid = dayPartDTO.dpid,
                Clid = dayPartDTO.clid,
                Name = dayPartDTO.name,
                Days = dayPartDTO.days
            });

            return affected != 0;
        }

        public async Task<bool> DeleteDayPart(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tbldp WHERE dpid = @Id", new { Id = id });

            return affected != 0;
        }
    }
}
