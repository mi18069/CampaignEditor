using AutoMapper;
using Dapper;
using Database.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

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
                "SELECT CAST(date AS DATE) FROM emsfiles.norecords ");

            return allRecords;
        }

        public async Task<bool> RunUpdateUnavailableDates()
        {
            using var connection = _context.GetConnection();

            var commandTimeout = 20; // Set the timeout value in seconds

            var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                      "WITH RECURSIVE all_dates AS( " +
                      "SELECT COALESCE(DATEADD(MONTH, -6, MAX(date)), CAST('2019-04-19' AS TIMESTAMP)) AS date " +
                      "FROM emsfiles.norecords " +
                      "UNION ALL " +
                      "SELECT date + INTERVAL '1' DAY " +
                      "FROM all_dates " +
                      "WHERE date < CURRENT_DATE " +
                    ") " +
                    "INSERT INTO emsfiles.norecords(date) " +
                    "SELECT date::DATE " +
                    "FROM all_dates " +
                    "WHERE NOT EXISTS( " +
                      "SELECT 1 " +
                      "FROM tblemsfileslist " +
                      "WHERE TO_DATE(SUBSTRING(filename, 1, 8), 'yyyyMMdd') = all_dates.date::DATE " +
                    ") " +
                      "OR NOT EXISTS( " +
                        "SELECT 1 " +
                        "FROM tblop4fileslist " +
                        "WHERE TO_DATE(SUBSTRING(filename, 1, 8), 'yyyyMMdd') = all_dates.date::DATE " +
                      ") " +
                      "OR NOT EXISTS( " +
                        "SELECT 1 " +
                        "FROM tbltv4fileslist " +
                        "WHERE TO_DATE(SUBSTRING(filename, 1, 8), 'yyyyMMdd') = all_dates.date::DATE " +
                      ") " +
                      "AND NOT EXISTS( " +
                        "SELECT 1 " +
                        "FROM emsfiles.norecords " +
                        "WHERE date::DATE = all_dates.date::DATE " +
                      "); "
                    ,
                    commandTimeout: commandTimeout));

            return affected != 0;
        }

        public async Task<bool> StartAMRCalculation(int cmpid, int minusTime, int plusTime, int xmpid=-1)
        {
            using var connection = _context.GetConnection();

            var commandTimeout = 900; // Set the timeout value in seconds

            int? id = null;
            if (xmpid != -1)
            {
                id = xmpid;
            }

            var affected = await connection.ExecuteAsync(
                new CommandDefinition(
                    "SELECT public.obrada_amr(@Cmpid, @MinusTime, @PlusTime, @Xmpid);",
                    new { Cmpid = cmpid, MinusTime = minusTime, PlusTime = plusTime, Xmpid = id },
                    commandTimeout: commandTimeout));

            return affected != 0;
        }
    }
}
