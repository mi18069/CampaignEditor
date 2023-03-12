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
                "INSERT INTO tblcmpgoals (cmpid, gbudget, ggrp, gins, grch_f1, grch_f2, grch) " +
                    "VALUES (@Cmpid, @Gbudget, @Ggrp, @Gins, @Grch_f1, @Grch_f2, @Grch)",
            new
            {
                Cmpid = goalsDTO.cmpid,
                GBudget = goalsDTO.budget,
                Ggrp = goalsDTO.grp,
                Gins = goalsDTO.ins,
                Grch_f1 = goalsDTO.rch_f1,
                Grch_f2 = goalsDTO.rch_f2,
                Grch = goalsDTO.rch
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
                "UPDATE tblcmpgoals SET cmpid = @Cmpid, gbudget = @GBudget, ggrp = @Ggrp, " +
                "gins = @Gins, grch_f1 = @Grch_f1, grch_f2 = @Grch_f2, grch = @Grch " +
                "WHERE cmpid = @Cmpid",
                new
                {
                    Cmpid = goalsDTO.cmpid,
                    GBudget = goalsDTO.budget,
                    Ggrp = goalsDTO.grp,
                    Gins = goalsDTO.ins,
                    Grch_f1 = goalsDTO.rch_f1,
                    Grch_f2 = goalsDTO.rch_f2,
                    Grch = goalsDTO.rch
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
