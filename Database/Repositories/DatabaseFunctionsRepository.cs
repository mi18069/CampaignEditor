using AutoMapper;
using Dapper;
using Database.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class DatabaseFunctionsRepository : IDatabaseFunctionsRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DatabaseFunctionsRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<DateTime>> GetAllUnavailableDates()
        {
            using var connection = _context.GetConnection();

            var allRecords = await connection.QueryAsync<DateTime>(
                "SELECT CAST(dateimport AS DATE) FROM emsfiles.norecords ");

            return allRecords;
        }

        public async Task<bool> RunUpdateUnavailableDates()
        {
            using var connection = _context.GetConnection();

            var commandTimeout = 20; // Set the timeout value in seconds

            var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                    "WITH RECURSIVE all_dates AS ( " +
                    "SELECT COALESCE(MAX(dateimport), CAST('2019-04-19' AS TIMESTAMP)) AS dateimport " +
                    "FROM emsfiles.norecords " +
                    "UNION ALL " +
                    "SELECT dateimport + INTERVAL '1' DAY " +
                    "FROM all_dates " +
                    "WHERE dateimport < CURRENT_DATE " +
                    ") " +
                    "INSERT INTO emsfiles.norecords(dateimport) " +
                    "SELECT dateimport::DATE " +
                    "FROM all_dates " +
                    "WHERE NOT EXISTS( " +
                      "SELECT 1 FROM tblemsfileslist WHERE dateimport::DATE = all_dates.dateimport::DATE " +
                                ") AND NOT EXISTS( " +
                      "SELECT 1 FROM tblop4fileslist WHERE dateimport::DATE = all_dates.dateimport::DATE " +
                                ") AND NOT EXISTS( " +
                      "SELECT 1 FROM tbltv4fileslist WHERE dateimport::DATE = all_dates.dateimport::DATE " +
                    ")",
                    commandTimeout: commandTimeout));

            return affected != 0;
        }

        public async Task<bool> StartAMRCalculation(int cmpid, int minusTime, int plusTime)
        {
            using var connection = _context.GetConnection();

            var commandTimeout = 900; // Set the timeout value in seconds

            var affected = await connection.ExecuteAsync(
                new CommandDefinition(
                    "SELECT public.obrada_amr(@Cmpid, @MinusTime, @PlusTime); " +
                    "UPDATE xmphist SET outlier = false",
                    new { Cmpid = cmpid, MinusTime = minusTime, PlusTime = plusTime },
                    commandTimeout: commandTimeout));

            return affected != 0;
        }
    }
}
