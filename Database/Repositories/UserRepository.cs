using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Entities;
using AutoMapper;
using Dapper;
using Database.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<int?> CreateUser(CreateUserDTO userDTO)
        {
            using var connection = _context.GetConnection();

            var newId = await connection.QuerySingleOrDefaultAsync<int?>(
                "INSERT INTO tblusers (usrname, usrpass, usrlevel, email, telefon, enabled, father, buy) " +
                    " VALUES (@Usrname, @Usrpass, @Usrlevel, @Email, @Telefon, @Enabled, @Father, @Buy) " +
                    " RETURNING usrid",
                new
                {
                    Usrname = userDTO.usrname,
                    Usrpass = userDTO.usrpass,
                    Usrlevel = userDTO.usrlevel,
                    Email = userDTO.email,
                    Telefon = userDTO.telefon,
                    Enabled = userDTO.enabled,
                    Father = userDTO.father,
                    Buy = userDTO.buy
                });

            return newId;
        }

        public async Task<UserDTO> GetUserById(int id)
        {
            using var connection = _context.GetConnection();

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM tblusers WHERE usrid = @Id", new { Id = id });

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> GetUserByUsername(string username)
        {
            using var connection = _context.GetConnection();

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM tblusers WHERE usrname = @Username", new { Username = username });

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsers()
        {
            using var connection = _context.GetConnection();

            var allUsers = await connection.QueryAsync<User>("SELECT * FROM tblusers");

            return _mapper.Map<IEnumerable<UserDTO>>(allUsers);
        }

        public async Task<IEnumerable<string>> GetAllUsernames()
        {
            using var connection = _context.GetConnection();

            var allUsernames = await connection.QueryAsync<string>("SELECT usrname FROM tblusers");

            return allUsernames;
        }

        public async Task<bool> UpdateUser(UpdateUserDTO userDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblusers SET usrname = @Usrname, usrpass = @Usrpass, usrlevel = @Usrlevel, " +
                "email = @Email, telefon = @Telefon, enabled = @Enabled, father = @Father, buy = @Buy " +
                "WHERE usrid = @Usrid",
                new
                {
                    Usrid = userDTO.usrid,
                    Usrname = userDTO.usrname,
                    Usrpass = userDTO.usrpass,
                    Usrlevel = userDTO.usrlevel,
                    Email = userDTO.email,
                    Telefon = userDTO.telefon,
                    Enabled = userDTO.enabled,
                    Father = userDTO.father,
                    Buy = userDTO.buy
                });

            return affected != 0;
        }

        public async Task<bool> DeleteUserById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblusers WHERE usrid = @Usrid", new { Usrid = id } );

            return affected != 0;
        }

        public async Task<bool> DeleteUserByUsername(string username)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblusers WHERE usrname = @Usrname", new { Usrname = username });

            return affected != 0;
        }

        public async Task<bool> CheckCredentials(string username, string password)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.QueryFirstAsync<int>(
                "SELECT COUNT(*) FROM tblusers WHERE usrname = @username AND usrpass = @password", new {username = username, password = password});

            return affected > 0;
        }

    }
}
