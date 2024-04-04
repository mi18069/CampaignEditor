using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ClientProgCoefDTO;
using Database.Entities;
using System;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ClientProgCoefRepository : IClientProgCoefRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public ClientProgCoefRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateClientProgCoef(ClientProgCoefDTO clientProgCoefDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblclprogcoef (clid, schid, progcoef) " +
                    "VALUES (@Clid, @Schid, @Progcoef)",
            new
            {
                Clid = clientProgCoefDTO.clid,
                Schid = clientProgCoefDTO.schid,
                Progcoef = clientProgCoefDTO.progcoef
            });

            return affected != 0;
        }   

        public async Task<ClientProgCoefDTO> GetClientProgCoef(int clid, int schid)
        {
            using var connection = _context.GetConnection();

            var clientProgCoef = await connection.QueryFirstOrDefaultAsync<ClientProgCoef>(
                "SELECT * FROM tblclprogcoef WHERE clid = @Clid AND schid = @Schid", 
                new { Clid = clid, Schid = schid });

            return _mapper.Map<ClientProgCoefDTO>(clientProgCoef);
        }

        public async Task<bool> UpdateClientProgCoef(ClientProgCoefDTO clientProgCoefDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblclprogcoef SET clid = @Clid, schid = @Schid, progcoef = @Progcoef " +
                "WHERE clid = @Clid AND schid = @Schid",
                new
                {
                    Clid = clientProgCoefDTO.clid,
                    Schid = clientProgCoefDTO.schid,
                    Progcoef = clientProgCoefDTO.progcoef
                });

            return affected != 0;
        }
        public async Task<bool> DeleteClientProgCoefByClientId(int clid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclprogcoef WHERE clid = @Clid", new { Clid = clid });

            return affected != 0;
        }

        public async Task<bool> DeleteClientProgCoef(int clid, int schid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclprogcoef WHERE clid = @Clid AND schid = @Schid", 
                new { Clid = clid, Schid = schid });

            return affected != 0;
        }


    }
}
