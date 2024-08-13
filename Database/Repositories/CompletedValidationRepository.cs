using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.CampaignDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class CompletedValidationRepository : ICompletedValidationRepository
    {

        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public CompletedValidationRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateCompValidation(CompletedValidation compValidation)
        {
            using var connection = _context.GetConnection();

            var success = await connection.QueryAsync(
                "INSERT INTO completedvalidation (cmpid, date, completed) " +
                    " VALUES (@Cmpid, @Date, @Completed)",
            new
            {
                Cmpid = compValidation.cmpid,
                Date = compValidation.date,
                Completed = compValidation.completed
            });

            return success != null;
        }

        public async Task<CompletedValidation> GetCompValidation(int cmpid, string date)
        {
            using var connection = _context.GetConnection();

            var compValidation = await connection.QueryFirstOrDefaultAsync<CompletedValidation>(
                "SELECT * FROM completedvalidation WHERE  cmpid = @Cmpid AND date = @Date",
            new
            {
                Cmpid = cmpid,
                Date = date
            });

            return compValidation;
        }

        public async Task<IEnumerable<CompletedValidation>> GetCompValidations(int cmpid)
        {
            using var connection = _context.GetConnection();

            var compValidations = await connection.QueryAsync<CompletedValidation>(
                "SELECT * FROM completedvalidation WHERE cmpid = @Cmpid",
            new
            {
                Cmpid = cmpid
            });

            return compValidations;
        }

        public async Task<bool> UpdateCompValidation(CompletedValidation compValidation)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE completedvalidation SET cmpid = @Cmpid, date = @Date, completed = @Completed " +         
                    " WHERE cmpid = @Cmpid AND date = @Date",
            new
            {
                Cmpid = compValidation.cmpid,
                Date = compValidation.date,
                Completed = compValidation.completed
            });

            return affected != 0;
        }

        public async Task<bool> DeleteCompValidation(int cmpid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM completedvalidation WHERE cmpid = @Cmpid", new { Cmpid = cmpid });

            return affected != 0;
        }

        public async Task<bool> DeleteCompValidation(int cmpid, string date)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM completedvalidation WHERE cmpid = @Cmpid AND date = @Date", new { Cmpid = cmpid, Date = date });

            return affected != 0;
        }
    }
}
