using Dapper;
using Database.Data;
using Database.Entities;
using System;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class DGConfigRepository : IDGConfigRepository
    {

        private readonly IDataContext _context;

        public DGConfigRepository(IDataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> CreateDGConfig(DGConfig dgConfig)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                @"INSERT INTO userdatagridconfig (usrid, clid, dgfor, dgexp, dgreal) 
                  VALUES (@Usrid, @Clid, @Dgfor, @Dgexp, @Dgreal) ", new
                {
                    Usrid = dgConfig.usrid,
                    Clid = dgConfig.clid,
                    Dgfor = dgConfig.dgfor,
                    Dgexp = dgConfig.dgexp,
                    Dgreal = dgConfig.dgreal
                });

            return affected != 0;
        }

        public async Task<DGConfig> GetDGConfig(int usrid, int clid)
        {
            using var connection = _context.GetConnection();

            var dgConfig = await connection.QueryFirstAsync<DGConfig>(
                @"SELECT * FROM userdatagridconfig WHERE usrid = @Usrid AND clid = @Clid ", 
                new { Usrid = usrid, Clid = clid });

            return dgConfig;
        }

        public async Task<bool> UpdateDGConfig(DGConfig dgConfig)
        {
            using var connection = _context.GetConnection();
            var affected = await connection.ExecuteAsync(
                @" UPDATE userdatagridconfig SET usrid = @Usrid, clid = @Clid,
                   dgfor = @Dgfor, dgexp = @Dgexp, dgreal = @Dgreal ",
                new {
                    Usrid = dgConfig.usrid,
                    Clid = dgConfig.clid,
                    Dgfor = dgConfig.dgfor,
                    Dgexp = dgConfig.dgexp,
                    Dgreal = dgConfig.dgreal
                });

            return affected != 0;
        }

        public async Task<bool> DeleteDGConfig(int usrid, int clid)
        {
            using var connection = _context.GetConnection();
            var affected = await connection.ExecuteAsync(
                @" DELETE FROM userdatagridconfig WHERE usrid = @Usrid AND clid = @Clid",
                new
                {
                    Usrid = usrid,
                    Clid = clid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteDGConfigByClid(int clid)
        {
            using var connection = _context.GetConnection();
            var affected = await connection.ExecuteAsync(
                @" DELETE FROM userdatagridconfig WHERE clid = @Clid",
            new
            {
                    Clid = clid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteDGConfigByUsrid(int usrid)
        {
            using var connection = _context.GetConnection();
            var affected = await connection.ExecuteAsync(
                @" DELETE FROM userdatagridconfig WHERE usrid = @Usrid",
                new
                {
                    Usrid = usrid
                });

            return affected != 0;
        }
    }
}
