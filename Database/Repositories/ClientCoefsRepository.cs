using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ClientCoefsDTO;
using Database.Entities;
using System;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ClientCoefsRepository : IClientCoefsRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public ClientCoefsRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateClientCoefs(ClientCoefsDTO clientCoefsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblclcoefs (clid, schid, progcoef, coefa, coefb) " +
                    "VALUES (@Clid, @Schid, @Progcoef, @CoefA, @CoefB)",
            new
            {
                Clid = clientCoefsDTO.clid,
                Schid = clientCoefsDTO.schid,
                Progcoef = clientCoefsDTO.progcoef,
                CoefA = clientCoefsDTO.coefA,
                CoefB = clientCoefsDTO.coefB
            });

            return affected != 0;
        }   

        public async Task<ClientCoefsDTO> GetClientCoefs(int clid, int schid)
        {
            using var connection = _context.GetConnection();

            var clientProgCoef = await connection.QueryFirstOrDefaultAsync<ClientCoefs>(
                "SELECT * FROM tblclcoefs WHERE clid = @Clid AND schid = @Schid", 
                new { Clid = clid, Schid = schid });

            return _mapper.Map<ClientCoefsDTO>(clientProgCoef);
        }

        public async Task<bool> UpdateClientCoefs(ClientCoefsDTO clientCoefsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblclcoefs SET clid = @Clid, schid = @Schid, progcoef = @Progcoef, " +
                "coefa = @CoefA, coefb = @CoefB " +
                "WHERE clid = @Clid AND schid = @Schid",
                new
                {
                    Clid = clientCoefsDTO.clid,
                    Schid = clientCoefsDTO.schid,
                    Progcoef = clientCoefsDTO.progcoef,
                    CoefA = clientCoefsDTO.coefA,
                    CoefB = clientCoefsDTO.coefB
                });

            return affected != 0;
        }
        public async Task<bool> DeleteClientCoefsByClientId(int clid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclcoefs WHERE clid = @Clid", new { Clid = clid });

            return affected != 0;
        }

        public async Task<bool> DeleteClientCoefs(int clid, int schid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclcoefs WHERE clid = @Clid AND schid = @Schid", 
                new { Clid = clid, Schid = schid });

            return affected != 0;
        }


    }
}
