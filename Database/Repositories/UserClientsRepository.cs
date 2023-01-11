using AutoMapper;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Entities;
using Dapper;
using Database.Data;
using Database.DTOs.UserClients;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class UserClientsRepository : IUserClientsRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public UserClientsRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateUserClients(UserClientsDTO userClientsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblclientsusers (cliid, usrid) VALUES (@Cliid, @Usrid)",
                new
                {
                    Cliid = userClientsDTO.cliid,
                    Usrid = userClientsDTO.usrid
                });

            return affected != 0;
        }   
        public async Task<UserClientsDTO> GetUserClientsByClientId(int id)
        {
            using var connection = _context.GetConnection();

            var userClient = await connection.QueryFirstOrDefaultAsync<UserClients>(
                "SELECT * FROM tblclientsusers WHERE cliid = @Id", new { Id = id });

            return _mapper.Map<UserClientsDTO>(userClient);
        }
        public async Task<UserClientsDTO> GetUserClientsByUserId(int id)
        {
            using var connection = _context.GetConnection();

            var userClient = await connection.QueryFirstOrDefaultAsync<UserClients>(
                "SELECT * FROM tblclientsusers WHERE usrid = @Id", new { Id = id });

            return _mapper.Map<UserClientsDTO>(userClient);
        }
        public async Task<bool> DeleteUserClientsByClientId(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclientsusers WHERE cliid = @Cliid", new { Cliid = id });

            return affected != 0;
        }

        public async Task<bool> DeleteUserClientsByUserId(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclientsusers WHERE usrid = @Usrid", new { Usrid = id });

            return affected != 0;
        }

        public async Task<bool> DeleteUserClients(int usrid, int clid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblclientsusers WHERE usrid = @Usrid AND cliid = @Cliid", new { Usrid = usrid, Cliid = clid });

            return affected != 0;
        }

        public async Task<IEnumerable<UserClientsDTO>> GetAllUserClients()
        {
            using var connection = _context.GetConnection();

            var allUsers = await connection.QueryAsync<UserClients>("SELECT * FROM tblclientsusers");

            return _mapper.Map<IEnumerable<UserClientsDTO>>(allUsers);
        }

        public async Task<IEnumerable<UserClientsDTO>> GetAllUserClientsByUserId(int id)
        {
            using var connection = _context.GetConnection();

            var allUsers = await connection.QueryAsync<UserClients>("SELECT * FROM tblclientsusers " +
                "WHERE usrid=@Usrid", new { Usrid = id });

            return _mapper.Map<IEnumerable<UserClientsDTO>>(allUsers);
        }

        public async Task<IEnumerable<UserClientsDTO>> GetAllUserClientsByClientId(int id)
        {
            using var connection = _context.GetConnection();

            var allUsers = await connection.QueryAsync<UserClients>("SELECT * FROM tblclientsusers " +
                "WHERE cliid=@Clid", new { Clid = id });

            return _mapper.Map<IEnumerable<UserClientsDTO>>(allUsers);
        }

    }
}
