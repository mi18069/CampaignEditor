using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.TargetDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class TargetRepository : ITargetRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public TargetRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateTarget(CreateTargetDTO targetDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tbltargets (targname, targown, targdesc, targdefi, targdefp) " +
                "VALUES (@Targname, @Targown, @Targdesc, @Targdefi, @Targdefp)",
            new
            {
                Targname = targetDTO.targname,
                Targown = targetDTO.targown,
                Targdesc = targetDTO.targdesc,
                Targdefi = targetDTO.targdefi,
                Targdefp = targetDTO.targdefp
            });

            return affected != 0;
        }
        public async Task<TargetDTO> GetTargetById(int id)
        {
            using var connection = _context.GetConnection();

            var target = await connection.QueryFirstOrDefaultAsync<Target>(
                "SELECT * FROM tbltargets WHERE targname = @Id", new { Id = id });

            return _mapper.Map<TargetDTO>(target);
        }
        public async Task<TargetDTO> GetTargetByName(string targname)
        {
            using var connection = _context.GetConnection();

            var target = await connection.QueryFirstOrDefaultAsync<Target>(
                "SELECT * FROM tbltargets WHERE targname = @Targname", new { Targname = targname });

            return _mapper.Map<TargetDTO>(target);
        }
        public async Task<IEnumerable<TargetDTO>> GetAllTargets()
        {
            using var connection = _context.GetConnection();

            var allTargets = await connection.QueryAsync<Target>("SELECT * FROM tbltargets");

            return _mapper.Map<IEnumerable<TargetDTO>>(allTargets);
        }
        public async Task<IEnumerable<TargetDTO>> GetAllClientTargets(int clientId)
        {
            using var connection = _context.GetConnection();

            var allTargets = await connection.QueryFirstAsync<Target>(
                "SELECT * FROM tbltargets WHERE targown == 0 OR targown == @ClientId",
                new { ClientId = clientId});

            return _mapper.Map<IEnumerable<TargetDTO>>(allTargets);
        }
        public async Task<bool> UpdateTarget(UpdateTargetDTO targetDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "Update tbltargets SET targname = @Targname, targown = @Targown, " +
                "targdesc = @Targdesc, targdefi = @Targdefi, targdefp = @Targdefp) " +
                "WHERE targid = @Targid",
            new
            {
                Targid = targetDTO.targid,
                Targname = targetDTO.targname,
                Targown = targetDTO.targown,
                Targdesc = targetDTO.targdesc,
                Targdefi = targetDTO.targdefi,
                Targdefp = targetDTO.targdefp
            });

            return affected != 0;
        }
        public async Task<bool> DeleteTargetById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tbltargets WHERE targid = @Id", new { Id = id });

            return affected != 0;
        }
        public async Task<bool> DeleteTargetByName(string targname)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tbltargets WHERE targname = @Targname", new { Targname = targname });

            return affected != 0;

        }
    }
}
