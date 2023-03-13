using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.GoalsDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class GoalsRepository : IGoalsRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GoalsRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateGoals(CreateGoalsDTO goalsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblcmpgoals (cmpid, budget, grp, ins, rch_f1, rch_f2, rch) " +
                    "VALUES (@Cmpid, @Budget, @Grp, @Ins, @Rch_f1, @Rch_f2, @Rch)",
            new
            {
                Cmpid = goalsDTO.cmpid,
                Budget = goalsDTO.budget,
                Grp = goalsDTO.grp,
                Ins = goalsDTO.ins,
                Rch_f1 = goalsDTO.rch_f1,
                Rch_f2 = goalsDTO.rch_f2,
                Rch = goalsDTO.rch
            });

            return affected != 0;
        }

        public async Task<GoalsDTO> GetGoalsByCmpid(int id)
        {
            using var connection = _context.GetConnection();

            var goals = await connection.QueryFirstOrDefaultAsync<Goals>(
                "SELECT * FROM tblcmpgoals WHERE cmpid = @Cmpid",
                new { Cmpid = id});

            return _mapper.Map<GoalsDTO>(goals);
        }

        public async Task<IEnumerable<GoalsDTO>> GetAllGoals()
        {
            using var connection = _context.GetConnection();

            var allGoals = await connection.QueryAsync<Goals>("SELECT * FROM tblcmpgoals");

            return _mapper.Map<IEnumerable<GoalsDTO>>(allGoals);
        }

        public async Task<bool> UpdateGoals(UpdateGoalsDTO goalsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblcmpgoals SET cmpid = @Cmpid, budget = @Budget, grp = @Grp, " +
                "ins = @Ins, rch_f1 = @Rch_f1, rch_f2 = @Rch_f2, rch = @Rch " +
                "WHERE cmpid = @Cmpid",
                new
                {
                    Cmpid = goalsDTO.cmpid,
                    Budget = goalsDTO.budget,
                    Grp = goalsDTO.grp,
                    Ins = goalsDTO.ins,
                    Rch_f1 = goalsDTO.rch_f1,
                    Rch_f2 = goalsDTO.rch_f2,
                    Rch = goalsDTO.rch
                });

            return affected != 0;
        }

        public async Task<bool> DeleteGoalsByCmpid(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcmpgoals WHERE cmpid = @Id", new { Id = id });

            return affected != 0;
        }
    }
}
