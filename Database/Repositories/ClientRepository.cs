using AutoMapper;
using CampaignEditor.Entities;
using Dapper;
using Database.Data;
using Database.DTOs.ClientDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public ClientRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateClient(CreateClientDTO clientDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblclients (clname, clactive, spid) VALUES (@Clname, @Clactive, @Spid)",
                new
                {
                    Clname = clientDTO.clname,
                    Clactive = clientDTO.clactive,
                    Spid = clientDTO.spid
                });

            return affected != 0;
        }

        public async Task<IEnumerable<ClientDTO>> GetAllClients()
        {
            using var connection = _context.GetConnection();

            var allClients = await connection.QueryAsync<Client>("SELECT * FROM tblclients");

            return _mapper.Map<IEnumerable<ClientDTO>>(allClients);
        }

        public async Task<ClientDTO> GetClientById(int id)
        {
            using var connection = _context.GetConnection();

            var client = await connection.QueryFirstOrDefaultAsync<Client>(
                "SELECT * FROM tblclients WHERE clid = @Id", new { Id = id });

            return _mapper.Map<ClientDTO>(client);
        }

        public async Task<ClientDTO> GetClientByName(string clname)
        {
            using var connection = _context.GetConnection();

            var client = await connection.QueryFirstOrDefaultAsync<Client>(
                "SELECT * FROM tblclients WHERE clname = @Clname", new { Clname = clname });

            return _mapper.Map<ClientDTO>(client);
        }

        public async Task<bool> UpdateClient(UpdateClientDTO clientDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblclients SET clid = @Clid, clname = @Clname, clactive = @Clactive, spid = @Spid " +
                "WHERE clid = @Clid",
                new
                {
                    Clid = clientDTO.clid,
                    Clname = clientDTO.clname,
                    Clactive = clientDTO.clactive,
                    Spid = clientDTO.spid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteClientById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclients WHERE clid = @Clid", new { Clid = id });

            return affected != 0;
        }

        public async Task<bool> DeleteClientByName(string clname)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclients WHERE clname = @Clname", new { Clname = clname });

            return affected != 0;
        }
    }
}
