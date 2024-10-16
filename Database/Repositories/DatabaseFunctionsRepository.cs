﻿using AutoMapper;
using Dapper;
using Database.Data;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        private async Task<bool> CheckPastDates()
        {
            using var connection = _context.GetConnection();

            var commandTimeout = 20; // Set the timeout value in seconds

            string sqlQuery = @"
                WITH RECURSIVE all_dates AS (
                  SELECT 
                    GREATEST(
                      CURRENT_DATE - INTERVAL '3' MONTH, 
                      CAST('2019-04-19' AS TIMESTAMP)
                    ) AS date
                  FROM emsfiles.norecords
                  UNION ALL
                  SELECT date + INTERVAL '1' DAY
                  FROM all_dates
                  WHERE date < CURRENT_DATE
                )
                DELETE FROM emsfiles.norecords
                WHERE date IN (
                  SELECT date::DATE
                  FROM all_dates
                  WHERE (
                    EXISTS (
                      SELECT 1 FROM tblemsfileslist WHERE TO_DATE(SUBSTRING(filename, 1, 8), 'yyyyMMdd') = all_dates.date::DATE
                    ) 
                    AND EXISTS (
                      SELECT 1 FROM tblop4fileslist WHERE TO_DATE(SUBSTRING(filename, 1, 8), 'yyyyMMdd') = all_dates.date::DATE
                    ) 
                    AND EXISTS (
                      SELECT 1 FROM tbltv4fileslist WHERE TO_DATE(SUBSTRING(filename, 1, 8), 'yyyyMMdd') = all_dates.date::DATE
                    ) 
                  )
                );
                ";

            var affected = await connection.ExecuteAsync(
            new CommandDefinition(sqlQuery, commandTimeout: commandTimeout));

            return affected != 0;
        }

        public async Task<bool> RunUpdateUnavailableDates()
        {
            using var connection = _context.GetConnection();

            var commandTimeout = 20; // Set the timeout value in seconds

            await CheckPastDates(); // Deleting dates in the last year if data for that dates are added

            var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                      "WITH RECURSIVE all_dates AS( " +
                      "SELECT COALESCE( max(date), CAST('2019-04-19' AS TIMESTAMP)) AS date " +
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

        public async Task<bool> CheckForecastPrerequisites(int cmpid)
        {
            using var connection = _context.GetConnection();

            // Check the number of rows with cmpid = cmpid in tables
            var cmptgtCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM tblcmptgt WHERE cmpid = @cmpid",
                new { cmpid });

            var cmpspotsCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM tblcmpspot WHERE cmpid = @cmpid",
                new { cmpid });

            var cmpchnCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM tblcmpchn WHERE cmpid = @cmpid",
                new { cmpid });



            return cmptgtCount > 0 && cmpspotsCount > 0 && cmpchnCount > 0;
        }

        public async Task<bool> StartReachCalculation(int cmpid, int segins = 20, int segbet = 60, bool delete = true, bool expr = true, string path = null)
        {
            using var connection = _context.GetConnection();
            var commandTimeout = 900; // Set the timeout value in seconds

            if (path == null)
            {
                var affected = await connection.ExecuteAsync(
                    "SELECT public.obrada_rch(@Cmpid, @Segins, @Segbet, @Delete); ",
                    new { Cmpid = cmpid, Segins = segins, Segbet = segbet, Delete = delete },
                    commandTimeout: commandTimeout);

                return affected != 0;
            }
            else {
                var affected = await connection.ExecuteAsync(
                    "SELECT public.obrada_rch(@Cmpid, @Segins, @Segbet, @Delete, @Expr, @Path); ",
                    new { Cmpid = cmpid, Segins = segins, Segbet = segbet, Delete = delete, Expr = expr, Path = path },
                    commandTimeout: commandTimeout);

                return affected != 0;
            }
        }

        public async Task<bool> StartRealizationFunction(int cmpid, int brandid, string sdate, string edate)
        {
            using var connection = _context.GetConnection();
            var commandTimeout = 900; // Set the timeout value in seconds


            var affected = await connection.ExecuteAsync(
                "SELECT public.\"Obrada_realizacije\"(@Cmpid, @Brandid, @Sdate, @Edate); ",
                new { Cmpid = cmpid, Brandid = brandid, Sdate = sdate, Edate = edate },
                commandTimeout: commandTimeout);

            return affected != 0;
            
        }

        public async Task<DateOnly> GetLastDateImport()
        {
            using var connection = _context.GetConnection();

            // Query to get the latest dateimport value
            var lastImportTimestamp = await connection.QuerySingleAsync<DateTime>(
                "SELECT MAX(dateimport) FROM emsfiles.tblspofileslist");

            // Convert the DateTime to DateOnly
            DateOnly lastImportDate = DateOnly.FromDateTime(lastImportTimestamp);

            return lastImportDate;
        }
    }
}
