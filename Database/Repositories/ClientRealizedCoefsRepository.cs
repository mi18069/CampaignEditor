using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ActivityDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ClientRealizedCoefsRepository : IClientRealizedCoefsRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public ClientRealizedCoefsRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateRealizedCoefs(ClientRealizedCoefs coefs)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblclreemscoef (clid, emsnum, progcoef)" +
                    "VALUES (@Clid, @Emsnum, @Progcoef)",
            new
            {
                Clid = coefs.clid,
                Emsnum = coefs.emsnum,
                Progcoef = coefs.progcoef
            });

            return affected != 0;
        }

        public async Task<ClientRealizedCoefs> GetRealizedCoefs(int clid, int emsnum)
        {
            using var connection = _context.GetConnection();

            var clientRealizedCoefs = await connection.QueryFirstOrDefaultAsync<ClientRealizedCoefs>(
                "SELECT * FROM tblclreemscoef WHERE clid = @Clid AND emsnum = @Emsnum", 
                new { Clid = clid, Emsnum = emsnum});

            return clientRealizedCoefs;
        }
        public async Task<IEnumerable<ClientRealizedCoefs>> GetAllClientRealizedCoefs(int clid)
        {
            using var connection = _context.GetConnection();

            var allClientRealizedCoefs = await connection.QueryAsync<ClientRealizedCoefs>
                ("SELECT * FROM tblclreemscoef WHERE clid = @Clid", new { Clid = clid });

            return allClientRealizedCoefs;
        }

        public async Task<bool> UpdateRealizedCoefs(ClientRealizedCoefs coefs)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblclreemscoef SET clid = @Clid, emsnum = @Emsnum, progcoef = @Progcoef " +
                "WHERE clid = @Clid AND emsnum = @Emsnum",
            new
            {
                Clid = coefs.clid,
                Emsnum = coefs.emsnum,
                Progcoef = coefs.progcoef
            });

            return affected != 0;
        }

        public async Task<bool> DeleteRealizedCoefs(int clid, int emsnum)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclreemscoef WHERE clid = @Clid AND emsnum = @Emsnum", 
                new { Clid = clid, Emsnum = emsnum });

            return affected != 0;
        }




    }
}
