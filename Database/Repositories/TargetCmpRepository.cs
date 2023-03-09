using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.TargetCmpDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class TargetCmpRepository : ITargetCmpRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public TargetCmpRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateTargetCmp(CreateTargetCmpDTO targetCmpDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblcmptgt (cmpid, targid, priority) " +
                    "VALUES (@Cmpid, @Targid, @Priority)",
            new
            {
                Cmpid = targetCmpDTO.cmpid,
                Targid = targetCmpDTO.targid,
                Priority = targetCmpDTO.priority
            });

            return affected != 0;
        }

        public async Task<TargetCmpDTO> GetTargetCmpByIds(int cmpid, int targid)
        {
            using var connection = _context.GetConnection();

            var targetCmp = await connection.QueryFirstOrDefaultAsync<TargetCmp>(
                "SELECT * FROM tblcmptgt WHERE cmpid = @Cmpid AND targid = @Targid",
                new { Cmpid = cmpid, Targid = targid });

            return _mapper.Map<TargetCmpDTO>(targetCmp);
        }

        public async Task<IEnumerable<TargetCmpDTO>> GetTargetCmpByCmpid(int id)
        {
            using var connection = _context.GetConnection();

            var targetCmps = await connection.QueryAsync<TargetCmp>(
                "SELECT * FROM tblcmptgt WHERE cmpid = @Id", new { Id = id });

            return _mapper.Map<IEnumerable<TargetCmpDTO>>(targetCmps);
        }

        public async Task<IEnumerable<TargetCmpDTO>> GetAllTargetCmps()
        {
            using var connection = _context.GetConnection();

            var allTargetCmps = await connection.QueryAsync<TargetCmp>("SELECT * FROM tblcmptgt");

            return _mapper.Map<IEnumerable<TargetCmpDTO>>(allTargetCmps);
        }

        public async Task<bool> UpdateTargetCmp(UpdateTargetCmpDTO targetCmpDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblcmptgt SET cmpid = @Cmpid, targid = @Targid, priority = @Priority " +
                "WHERE cmpid = @Cmpid AND targid = @Targid",
                new
                {
                    Cmpid = targetCmpDTO.cmpid,
                    Targid = targetCmpDTO.targid,
                    Priority = targetCmpDTO.priority
                });

            return affected != 0;
        }

        public async Task<bool> DeleteTargetCmpByCmpid(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcmptgt WHERE cmpid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteTargetCmpByIds(int cmpid, int targid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcmptgt WHERE cmpid = @Id AND targid = @Targid",
                new { Id = cmpid, Targid = targid });

            return affected != 0;
        }
    }
}
